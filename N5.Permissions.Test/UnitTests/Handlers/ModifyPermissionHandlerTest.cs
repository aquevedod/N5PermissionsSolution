using Xunit;
using Moq;
using N5.Permissions.Application.Handlers;
using N5.Permissions.Application.Commands;
using N5.Permissions.Domain.Entities;
using N5.Permissions.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Threading;
using N5.Permissions.Application.Common.Interfaces;
using N5.Permissions.Application.Common.Models;
using System;
using FluentAssertions;

namespace N5.Permissions.Tests.HandlerTests
{
    public class ModifyPermissionHandlerTests
    {
        private readonly Mock<IPermissionRepository> _mockPermissionRepo;
        private readonly Mock<IPermissionTypeRepository> _mockPermissionTypeRepo;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ILogger<ModifyPermissionHandler>> _mockLogger;
        private readonly Mock<IPermissionIndexer> _mockIndexer;
        private readonly Mock<IKafkaProducer> _mockKafkaProducer;
        private readonly ModifyPermissionHandler _handler;

        public ModifyPermissionHandlerTests()
        {
            _mockPermissionRepo = new Mock<IPermissionRepository>();
            _mockPermissionTypeRepo = new Mock<IPermissionTypeRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockLogger = new Mock<ILogger<ModifyPermissionHandler>>();
            _mockIndexer = new Mock<IPermissionIndexer>();
            _mockKafkaProducer = new Mock<IKafkaProducer>();

            _handler = new ModifyPermissionHandler(
                _mockPermissionRepo.Object,
                _mockPermissionTypeRepo.Object,
                _mockUnitOfWork.Object,
                _mockLogger.Object,
                _mockIndexer.Object,
                _mockKafkaProducer.Object);
        }

        [Fact]
        public async Task Handle_ValidRequest_ReturnsTrueAndUpdatesPermission()
        {
            // Arrange
            var request = new ModifyPermissionCommand
            {
                Id = 1,
                EmployeeForename = "Updated",
                EmployeeSurname = "Name",
                PermissionTypeId = 2,
                PermissionDate = DateTime.UtcNow
            };

            var existingPermission = new Permission
            {
                Id = 1,
                EmployeeForename = "Original",
                EmployeeSurname = "Name",
                PermissionTypeId = 1,
                PermissionDate = DateTime.UtcNow.AddDays(-1)
            };

            _mockPermissionRepo.Setup(x => x.GetByIdAsync(request.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingPermission);

            _mockPermissionTypeRepo.Setup(x => x.GetByIdAsync(request.PermissionTypeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PermissionType { Id = 2 });

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
            existingPermission.EmployeeForename.Should().Be(request.EmployeeForename);
            existingPermission.PermissionTypeId.Should().Be(request.PermissionTypeId);
            _mockPermissionRepo.Verify(x => x.Update(existingPermission), Times.Once);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_PermissionNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var request = new ModifyPermissionCommand { Id = 99 };

            _mockPermissionRepo.Setup(x => x.GetByIdAsync(request.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Permission)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _handler.Handle(request, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_PermissionTypeNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var request = new ModifyPermissionCommand
            {
                Id = 1,
                PermissionTypeId = 99
            };

            _mockPermissionRepo.Setup(x => x.GetByIdAsync(request.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Permission());

            _mockPermissionTypeRepo.Setup(x => x.GetByIdAsync(request.PermissionTypeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((PermissionType)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _handler.Handle(request, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldLogInformation()
        {
            // Arrange
            var request = new ModifyPermissionCommand
            {
                Id = 1,
                EmployeeForename = "Test",
                EmployeeSurname = "User",
                PermissionTypeId = 1,
                PermissionDate = DateTime.UtcNow
            };

            _mockPermissionRepo.Setup(x => x.GetByIdAsync(request.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Permission());

            _mockPermissionTypeRepo.Setup(x => x.GetByIdAsync(request.PermissionTypeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PermissionType());

            // Act
            await _handler.Handle(request, CancellationToken.None);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Modified permission for")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}