using Xunit;
using Moq;
using N5.Permissions.Application.Handlers;
using N5.Permissions.Application.Queries;
using N5.Permissions.Domain.Entities;
using N5.Permissions.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using N5.Permissions.Application.DTO;
using System;
using System.ComponentModel.DataAnnotations;
using N5.Permissions.Application.Common.Interfaces;
using System.Security;

namespace N5.Permissions.Tests.HandlerTests
{
    public class GetPermissionHandlerTests
    {
        private readonly Mock<IPermissionRepository> _mockRepo;
        private readonly GetPermissionHandler _handler;

        public GetPermissionHandlerTests()
        {
            _mockRepo = new Mock<IPermissionRepository>();
            var mockLogger = new Mock<ILogger<GetPermissionHandler>>();
            var mockIndexer = new Mock<IPermissionIndexer>();
            var mockKafka = new Mock<IKafkaProducer>();

            _handler = new GetPermissionHandler(
                _mockRepo.Object,
                mockLogger.Object,
                mockIndexer.Object,
                mockKafka.Object);
        }

        [Fact]
        public async Task Handle_WithPermissions_ReturnsList()
        {
            // Arrange
            var permissionsDto = new List<PermissionDto>
            {
                new PermissionDto { Id = 1,
                    EmployeeForename = "John",
                    PermissionTypeDescription = "Sick Leave",
                }
            };

            var permissions = permissionsDto.Select(p => new Permission
            {
                Id = p.Id,
                EmployeeForename = p.EmployeeForename,
                EmployeeSurname = p.EmployeeSurname,
                PermissionDate = p.PermissionDate,
                PermissionTypeId = p.PermissionTypeId,
                PermissionTypeNavigation = new PermissionType
                {
                    Id = p.PermissionTypeId,
                    Description = p.PermissionTypeDescription
                }
            }).ToList();

            _mockRepo.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(permissions);

            // Act
            var result = await _handler.Handle(new GetPermissionsQuery(), CancellationToken.None);

            // Assert
            Assert.Single(result);
            Assert.Equal("John", result[0].EmployeeForename);
        }
    }
}