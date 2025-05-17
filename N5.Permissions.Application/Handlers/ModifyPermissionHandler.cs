using MediatR;
using Microsoft.Extensions.Logging;
using N5.Permissions.Application.Commands;
using N5.Permissions.Application.Common.Interfaces;
using N5.Permissions.Application.Common.Models;
using N5.Permissions.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N5.Permissions.Application.Handlers
{
    public class ModifyPermissionHandler :IRequestHandler<ModifyPermissionCommand, bool>
    {
        private readonly IPermissionRepository _permissionRepository;
        private readonly IPermissionTypeRepository _permissionTypeRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ModifyPermissionHandler> _logger;
        private readonly IPermissionIndexer _indexer;
        private readonly IKafkaProducer _kafkaProducer;

        public ModifyPermissionHandler(IPermissionRepository permissionRepository,
            IPermissionTypeRepository permissionTypeRepository,
            IUnitOfWork unitOfWork, ILogger<ModifyPermissionHandler> logger,
            IPermissionIndexer indexer, IKafkaProducer kafkaProducer)
        {
            _permissionRepository = permissionRepository;
            _permissionTypeRepository = permissionTypeRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _indexer = indexer;
            _kafkaProducer = kafkaProducer;
        }
        public async Task<bool> Handle(ModifyPermissionCommand request, CancellationToken cancellationToken)
        {
            var permission = await _permissionRepository.GetByIdAsync(request.Id, cancellationToken) ?? throw new KeyNotFoundException($"Permission with ID {request.Id} not found.");
            _ = await _permissionTypeRepository.GetByIdAsync(request.PermissionTypeId, cancellationToken) ?? throw new KeyNotFoundException($"Permission type with ID {request.PermissionTypeId} not found.");

            permission.EmployeeForename = request.EmployeeForename;
            permission.EmployeeSurname = request.EmployeeSurname;
            permission.PermissionTypeId = request.PermissionTypeId;
            permission.PermissionDate = request.PermissionDate;

            _permissionRepository.Update(permission);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Modified permission for {Forename} {Surname} (TypeId: {PermissionTypeId}, Date: {PermissionDate})",
                request.EmployeeForename, request.EmployeeSurname, request.PermissionTypeId, request.PermissionDate);

            await _indexer.IndexAsync(new PermissionDocument
            {
                Id = permission.Id,
                EmployeeForename = permission.EmployeeForename,
                EmployeeSurname = permission.EmployeeSurname,
                PermissionDate = permission.PermissionDate,
                PermissionTypeId = permission.PermissionTypeId,
                Operation = "modify"
            }, cancellationToken);

            await _kafkaProducer.ProduceAsync(new PermissionEvent
            {
                Id = Guid.NewGuid(),
                Operation = "modify"
            }, cancellationToken);


            return true;
        }
    }
}
