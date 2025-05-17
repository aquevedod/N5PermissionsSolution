using N5.Permissions.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N5.Permissions.Domain.Interfaces
{
    public interface IPermissionRepository
    {
        /// <summary>
        /// Adds a new permission request to the database.
        /// </summary>
        /// <param name="permission">The permission request to add.</param>
        Task AddAsync(Permission permission, CancellationToken cancellationToken);
        /// <summary>
        /// Gets all permission requests from the database.
        /// </summary>
        Task<List<Permission>> GetAllAsync(CancellationToken cancellationToken);
        /// <summary>
        /// Gets a specific permission request by its ID.
        /// </summary>
        /// <param name="id">The ID of the permission request.</param>
        Task<Permission?> GetByIdAsync(int id, CancellationToken cancellationToken);

        void Update(Permission permission);
    }
}
