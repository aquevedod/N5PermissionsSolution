using Microsoft.EntityFrameworkCore;
using N5.Permissions.Domain.Entities;
using N5.Permissions.Domain.Interfaces;
using N5.Permissions.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N5.Permissions.Infrastructure.Repositories
{
    public class PermissionTypeRepository : IPermissionTypeRepository
    {
        private readonly PermissionsDbContext _context;
        public PermissionTypeRepository(PermissionsDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(Domain.Entities.PermissionType permissionType, CancellationToken cancellationToken)
        {
            await _context.PermissionTypes.AddAsync(permissionType);
        }

        public Task<List<Domain.Entities.PermissionType>> GetAllAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<PermissionType?> GetByDescriptionAsync(string description, CancellationToken cancellationToken)
        {
            return await _context.PermissionTypes.Where(x => x.Description == description).FirstOrDefaultAsync();
        }

        public async Task<Domain.Entities.PermissionType?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            return await _context.PermissionTypes.Where(x => x.Id.Equals(id)).FirstOrDefaultAsync();
        }

        public void Update(Domain.Entities.PermissionType permissionType)
        {
            throw new NotImplementedException();
        }
    }
}
