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
    public class PermissionRepository : IPermissionRepository
    {
        private readonly PermissionsDbContext _context;
        public PermissionRepository(PermissionsDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Permission permission, CancellationToken cancellationToken)
        {
            await _context.Permissions.AddAsync(permission);
        }

        public async Task<List<Permission>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await _context.Permissions
                .Include(p => p.PermissionTypeNavigation)
                .ToListAsync(cancellationToken);
        }

        public async Task<Permission?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            return await _context.Permissions
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public void Update(Permission permission)
        {
            _context.Permissions.Update(permission);
        }
    }
}
