using N5.Permissions.Domain.Interfaces;
using N5.Permissions.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N5.Permissions.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly PermissionsDbContext _context;

        public UnitOfWork(PermissionsDbContext context)
        {
            _context = context;
        }
        public async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
