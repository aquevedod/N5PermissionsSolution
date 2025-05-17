using MediatR;
using Microsoft.Extensions.Logging;
using N5.Permissions.Application.Common.Interfaces;
using N5.Permissions.Application.Common.Models;
using N5.Permissions.Application.DTO;
using N5.Permissions.Application.Queries;
using N5.Permissions.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;


namespace N5.Permissions.Application.Handlers
{
    public class GetPermissionHandler : IRequestHandler<GetPermissionsQuery, List<PermissionDto>>
    {
        private readonly IPermissionRepository _permissionRepository;
        private readonly ILogger<GetPermissionHandler> _logger;
        private readonly IPermissionIndexer _indexer;
        private readonly IKafkaProducer _kafkaProducer;

        public GetPermissionHandler(IPermissionRepository permissionRepository,
            ILogger<GetPermissionHandler> logger, IPermissionIndexer indexer,
            IKafkaProducer kafkaProducer)
        {
            _permissionRepository = permissionRepository;
            _logger = logger;
            _indexer = indexer;
            _kafkaProducer = kafkaProducer;
        }
        public async Task<List<PermissionDto>> Handle(GetPermissionsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("GetPermissions executed");
                var permissions = await _permissionRepository.GetAllAsync(cancellationToken);

                if (permissions == null || permissions.Count == 0)
                {
                    _logger.LogWarning("No permissions found.");
                    throw new ValidationException("No permissions found.");
                }

                foreach (var permission in permissions)
                {
                    var doc = new PermissionDocument
                    {
                        Id = permission.Id,
                        EmployeeForename = permission.EmployeeForename,
                        EmployeeSurname = permission.EmployeeSurname,
                        PermissionDate = permission.PermissionDate,
                        PermissionTypeId = permission.PermissionTypeId,
                        Operation = "get"
                    };
                    await _indexer.IndexAsync(doc, cancellationToken);
                }
                _logger.LogInformation("Returned {Count} permissions", permissions.Count);

                await _kafkaProducer.ProduceAsync(new PermissionEvent
                {
                    Id = Guid.NewGuid(),
                    Operation = "get"
                }, cancellationToken);


                return permissions.Select(p => new PermissionDto
                {
                    Id = p.Id,
                    EmployeeForename = p.EmployeeForename,
                    EmployeeSurname = p.EmployeeSurname,
                    PermissionDate = p.PermissionDate,
                    PermissionTypeId = p.PermissionTypeId,
                    PermissionTypeDescription = p.PermissionTypeNavigation.Description
                }).ToList();

                
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred while retrieving permissions: {Message}", ex.Message);
                throw new Exception("An error occurred while retrieving permissions.", ex);
            }
        }
    }
}
