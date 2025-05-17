using MediatR;
using Microsoft.Extensions.Logging;
using N5.Permissions.Application.DTO;
using N5.Permissions.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N5.Permissions.Application.Handlers
{
    public class CreatePermissionTypeHandler : IRequestHandler<CreatePermissionTypeCommand, int>
    {
        private readonly IPermissionTypeRepository _permissionTypeRepository;
        private readonly IUnitOfWork _unit;
        private readonly ILogger<CreatePermissionTypeHandler> _logger;

        public CreatePermissionTypeHandler(IPermissionTypeRepository permissionTypeRepository, IUnitOfWork unitOfWork,
            ILogger<CreatePermissionTypeHandler> logger)
        {
            _permissionTypeRepository = permissionTypeRepository;
            _unit = unitOfWork;
            _logger = logger;
        }

        public async Task<int> Handle(CreatePermissionTypeCommand request, CancellationToken cancellationToken)
        {
            var existingPermissionType = await _permissionTypeRepository.GetByDescriptionAsync(request.Description, cancellationToken);

            if (existingPermissionType != null)
            {
                _logger.LogWarning("Permission type '{Description}' already exists", request.Description);
                throw new ValidationException($"Permission type with description '{request.Description}' already exists.");
            }

            var permissionType = new Domain.Entities.PermissionType
            {
                Description = request.Description
            };

            await _permissionTypeRepository.AddAsync(permissionType, cancellationToken);
            await _unit.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("CreatePermissionType executed for {Description}", request.Description);

            return permissionType.Id;
        }

    }
}
