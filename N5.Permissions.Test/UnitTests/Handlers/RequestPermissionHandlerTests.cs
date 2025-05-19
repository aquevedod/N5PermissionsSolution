using Xunit;
using Moq;
using N5.Permissions.Application.Handlers;
using N5.Permissions.Application.Common.Interfaces;
using N5.Permissions.Domain.Entities;
using N5.Permissions.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Threading;
using System;
using N5.Permissions.Application.Common.Models;
using N5.Permissions.Domain.DTO;
using FluentAssertions;
using Serilog;

namespace N5.Permissions.Tests.HandlerTests
{
    public class RequestPermissionHandlerTests
    {
        private readonly Mock<IPermissionRepository> _mockPermissionRepo;
        private readonly Mock<IPermissionTypeRepository> _mockPermissionTypeRepo;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ILogger<RequestPermissionHandler>> _mockLogger;
        private readonly Mock<IPermissionIndexer> _mockIndexer;
        private readonly Mock<IKafkaProducer> _mockKafkaProducer;
        private readonly RequestPermissionHandler _handler;

        public RequestPermissionHandlerTests()
        {
            _mockPermissionRepo = new Mock<IPermissionRepository>();
            _mockPermissionTypeRepo = new Mock<IPermissionTypeRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockLogger = new Mock<ILogger<RequestPermissionHandler>>();
            _mockIndexer = new Mock<IPermissionIndexer>();
            _mockKafkaProducer = new Mock<IKafkaProducer>();

            _handler = new RequestPermissionHandler(
                _mockPermissionRepo.Object,
                _mockPermissionTypeRepo.Object,
                _mockUnitOfWork.Object,
                _mockLogger.Object,
                _mockIndexer.Object,
                _mockKafkaProducer.Object);
        }

        [Fact]
        public async Task Handle_ValidRequest_ReturnsPermissionId()
        {
            // Arrange
            var request = new RequestPermissionCommand("John", "Doe", 1);
            var permissionType = new PermissionType { Id = 1, Description = "Test" };

            _mockPermissionTypeRepo.Setup(x => x.GetByIdAsync(request.PermissionTypeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(permissionType);

            _mockPermissionRepo.Setup(x => x.AddAsync(It.IsAny<Permission>(), It.IsAny<CancellationToken>()))
                .Callback<Permission, CancellationToken>((p, _) => p.Id = 123) // Assign ID
                .Returns(Task.CompletedTask);

            _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().Be(123); // Now it returns the mocked ID
            _mockPermissionTypeRepo.Verify(x => x.GetByIdAsync(request.PermissionTypeId, It.IsAny<CancellationToken>()), Times.Once);
            _mockPermissionRepo.Verify(x => x.AddAsync(
                It.Is<Permission>(p =>
                    p.EmployeeForename == request.EmployeeForename &&
                    p.EmployeeSurname == request.EmployeeSurname),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_InvalidPermissionType_ThrowsKeyNotFoundException()
        {
            // Arrange
            var request = new RequestPermissionCommand("John", "Doe", 99);

            _mockPermissionTypeRepo.Setup(x => x.GetByIdAsync(request.PermissionTypeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((PermissionType)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _handler.Handle(request, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldIndexPermissionDocument()
        {
            // Arrange
            var request = new RequestPermissionCommand("Alice", "Johnson", 3);
            var permissionType = new PermissionType { Id = 3, Description = "Test" };

            _mockPermissionTypeRepo.Setup(x => x.GetByIdAsync(request.PermissionTypeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(permissionType);

            PermissionDocument capturedDocument = null;
            _mockIndexer.Setup(x => x.IndexAsync(It.IsAny<PermissionDocument>(), It.IsAny<CancellationToken>()))
                .Callback<PermissionDocument, CancellationToken>((doc, _) => capturedDocument = doc)
                .Returns(Task.CompletedTask);

            // Act
            await _handler.Handle(request, CancellationToken.None);

            // Assert
            capturedDocument.Should().NotBeNull();
            capturedDocument.EmployeeForename.Should().Be("Alice");
            capturedDocument.Operation.Should().Be("request");
            capturedDocument.PermissionTypeId.Should().Be(3);
        }

        [Fact]
        public async Task Handle_ShouldProduceKafkaEvent()
        {
            // Arrange
            var request = new RequestPermissionCommand("Bob", "Brown", 4);
            var permissionType = new PermissionType { Id = 4, Description = "Test" };

            _mockPermissionTypeRepo.Setup(x => x.GetByIdAsync(request.PermissionTypeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(permissionType);

            PermissionEvent capturedEvent = null;
            _mockKafkaProducer.Setup(x => x.ProduceAsync(It.IsAny<PermissionEvent>(), It.IsAny<CancellationToken>()))
                .Callback<PermissionEvent, CancellationToken>((evt, _) => capturedEvent = evt)
                .Returns(Task.CompletedTask);

            // Act
            await _handler.Handle(request, CancellationToken.None);

            // Assert
            capturedEvent.Should().NotBeNull();
            capturedEvent.Operation.Should().Be("request");
            capturedEvent.Id.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Handle_WhenSaveFails_ShouldThrow()
        {
            // Arrange
            var request = new RequestPermissionCommand("Save", "Error", 7);
            var permissionType = new PermissionType { Id = 7, Description = "Test" };

            _mockPermissionTypeRepo.Setup(x => x.GetByIdAsync(request.PermissionTypeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(permissionType);

            _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            Func<Task> act = () => _handler.Handle(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("Database error");
        }
    }
}