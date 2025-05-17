using N5.Permissions.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N5.Permissions.Domain.Interfaces
{
    public interface IPermissionTypeRepository
    {
        /// <summary>
        /// Adds a new permission type to the database.
        /// </summary>
        /// <param name="permissionType">The permission type to add.</param>
        /// 
        Task AddAsync(PermissionType permissionType, CancellationToken cancellationToken);
        /// <summary>
        /// Gets all permission types from the database.
        /// </summary>
        Task<List<PermissionType>> GetAllAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Gets a specific permission type by its ID.
        /// </summary>
        /// <param name="id">The ID of the permission type.</param>
        Task<PermissionType?> GetByIdAsync(int id, CancellationToken cancellationToken);

        Task<PermissionType?> GetByDescriptionAsync(string description, CancellationToken cancellationToken);

        void Update(PermissionType permissionType);
    }
}
