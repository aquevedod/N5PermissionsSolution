using MediatR;
using Microsoft.Extensions.Logging;
using N5.Permissions.Application.Common.Interfaces;
using N5.Permissions.Application.Common.Models;
using N5.Permissions.Domain.DTO;
using N5.Permissions.Domain.Entities;
using N5.Permissions.Domain.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N5.Permissions.Application.Handlers
{
    public class RequestPermissionHandler : IRequestHandler<RequestPermissionCommand, int>
    {
        private readonly IPermissionRepository _permissionRepository;
        private readonly IPermissionTypeRepository _permissionTypeRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<RequestPermissionHandler> _logger;
        private readonly IPermissionIndexer _indexer;
        private readonly IKafkaProducer _kafkaProducer;


        public RequestPermissionHandler(IPermissionRepository permissionRepository,
            IPermissionTypeRepository permissionTypeRepository,
            IUnitOfWork unitOfWork,
            ILogger<RequestPermissionHandler> logger,
            IPermissionIndexer indexer,
            IKafkaProducer kafkaProducer)
        {
            _permissionRepository = permissionRepository;
            _permissionTypeRepository = permissionTypeRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _indexer = indexer;
            _kafkaProducer = kafkaProducer;
        }
        public async Task<int> Handle(RequestPermissionCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Requesting permission for {EmployeeForename} {EmployeeSurname}",
                request.EmployeeForename, request.EmployeeSurname);

            _ = await _permissionTypeRepository.GetByIdAsync(request.PermissionTypeId, cancellationToken)
                ?? throw new KeyNotFoundException($"Permission type with ID {request.PermissionTypeId} not found.");

            var permission = new Permission
            {
                EmployeeForename = request.EmployeeForename,
                EmployeeSurname = request.EmployeeSurname,
                PermissionTypeId = request.PermissionTypeId,
                PermissionDate = DateTime.UtcNow
            };

            await _permissionRepository.AddAsync(permission, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation($"RequestPermission executed for Employee:" +
                $"{request.EmployeeForename} {request.EmployeeSurname}, TypeId: {request.PermissionTypeId}");

            await _indexer.IndexAsync(new PermissionDocument
            {
                Id = permission.Id,
                EmployeeForename = permission.EmployeeForename,
                EmployeeSurname = permission.EmployeeSurname,
                PermissionDate = permission.PermissionDate,
                PermissionTypeId = permission.PermissionTypeId,
                Operation = "request"
            }, cancellationToken);

            await _kafkaProducer.ProduceAsync(new PermissionEvent
            {
                Id = Guid.NewGuid(),
                Operation = "request"
            }, cancellationToken);



            return permission.Id;
        }
    }
}
