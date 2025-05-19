using Xunit;
using Microsoft.EntityFrameworkCore;
using N5.Permissions.Infrastructure.Persistence;
using N5.Permissions.Application.Handlers;
using N5.Permissions.Application.Queries;
using N5.Permissions.Domain.Entities;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using N5.Permissions.Application.Common.Interfaces;
using N5.Permissions.Infrastructure.Repositories;

namespace N5.Permissions.Tests.IntegrationTests
{
    public class GetPermissionHandlerIntegrationTests : IDisposable
    {
        private readonly PermissionsDbContext _context;
        private readonly GetPermissionHandler _handler;

        public GetPermissionHandlerIntegrationTests()
        {
            // Configurar base de datos en memoria
            var options = new DbContextOptionsBuilder<PermissionsDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;

            _context = new PermissionsDbContext(options);

            // Datos de prueba
            _context.Permissions.Add(new Permission
            {
                Id = 1,
                EmployeeForename = "Test",
                PermissionTypeNavigation = new PermissionType { Description = "TestType" }
            });
            _context.SaveChanges();

            // Mocks para dependencias externas (simplificados)
            var mockLogger = new Mock<ILogger<GetPermissionHandler>>();
            var mockIndexer = new Mock<IPermissionIndexer>();
            var mockKafka = new Mock<IKafkaProducer>();

            _handler = new GetPermissionHandler(
                new PermissionRepository(_context),
                mockLogger.Object,
                mockIndexer.Object,
                mockKafka.Object);
        }

        [Fact]
        public async Task Handle_WithData_ReturnsPermissions()
        {
            // Act
            var result = await _handler.Handle(new GetPermissionsQuery(), CancellationToken.None);

            // Assert
            Assert.Single(result);
            Assert.Equal("Test", result[0].EmployeeForename);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}