using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using N5.Permissions.API.Controllers;
using N5.Permissions.Application.Commands;
using N5.Permissions.Application.Queries;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using N5.Permissions.Domain.DTO;
using System;
using System.Linq;
using FluentAssertions;
using N5.Permissions.Application.DTO;

namespace N5.Permissions.Tests.ControllerTests
{
    public class PermissionControllerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly PermissionController _controller;

        public PermissionControllerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _controller = new PermissionController(_mockMediator.Object);
        }

        [Fact]
        public async Task RequestPermission_ValidCommand_ReturnsOkWithId()
        {
            // Arrange
            var command = new RequestPermissionCommand("Teofilo", "Gutierrez", 1);
            _mockMediator.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(123);

            // Act
            var result = await _controller.RequestPermission(command);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(new { Id = 123 });
        }

        [Fact]
        public async Task RequestPermission_PermissionTypeNotFound_ReturnsNotFound()
        {
            // Arrange
            var command = new RequestPermissionCommand("Teofilo", "Gutierrez", 999);
            _mockMediator.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new KeyNotFoundException("Permission type not found"));

            // Act
            var result = await _controller.RequestPermission(command);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull(); // Ensure notFoundResult is not null
            notFoundResult!.Value.Should().Be("Permission type not found");
        }

        [Fact]
        public async Task GetPermissions_WithResults_ReturnsOkWithPermissions()
        {
            // Arrange
            var permissions = new List<PermissionDto>
            {
                new() { Id = 1, EmployeeForename = "Teofilo" },
                new() { Id = 2, EmployeeForename = "Jane" }
            };

            _mockMediator.Setup(m => m.Send(It.IsAny<GetPermissionsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(permissions);

            // Act
            var result = await _controller.GetPermissions(CancellationToken.None);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(permissions);
        }

        [Fact]
        public async Task GetPermissions_NoResults_ReturnsNotFound()
        {
            // Arrange
            _mockMediator.Setup(m => m.Send(It.IsAny<GetPermissionsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PermissionDto>());

            // Act
            var result = await _controller.GetPermissions(CancellationToken.None);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull(); // Ensure notFoundResult is not null
            notFoundResult.Value.Should().Be("No permissions found.");
        }

        [Fact]
        public async Task GetPermissions_Exception_ReturnsInternalServerError()
        {
            // Arrange
            _mockMediator.Setup(m => m.Send(It.IsAny<GetPermissionsQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetPermissions(CancellationToken.None);

            // Assert
            var statusCodeResult = result.Should().BeOfType<ObjectResult>().Subject;
            statusCodeResult.StatusCode.Should().Be(500);
            statusCodeResult.Value.Should().Be("Internal server error: Database error");
        }

        [Fact]
        public async Task ModifyPermission_IdsMatch_ReturnsOk()
        {
            // Arrange
            var command = new ModifyPermissionCommand { Id = 1 };
            _mockMediator.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.ModifyPermission(1, command, CancellationToken.None);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().Be("Permission updated");
        }

        [Fact]
        public async Task ModifyPermission_IdsDontMatch_ReturnsBadRequest()
        {
            // Arrange
            var command = new ModifyPermissionCommand { Id = 1 };

            // Act
            var result = await _controller.ModifyPermission(2, command, CancellationToken.None);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().Be("ID in the URL does not match ID in the body.");
        }

        [Fact]
        public async Task ModifyPermission_NotFound_ReturnsNotFound()
        {
            // Arrange
            var command = new ModifyPermissionCommand { Id = 999 };
            _mockMediator.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new KeyNotFoundException("Permission not found"));

            // Act
            var result = await _controller.ModifyPermission(999, command, CancellationToken.None);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull(); // Ensure notFoundResult is not null
            notFoundResult.Value.Should().Be("Permission not found");
        }
    }
}