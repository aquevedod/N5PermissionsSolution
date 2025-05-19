using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N5.Permissions.Infrastructure.Persistence
{
    public class PermissionsDbContextFactory : IDesignTimeDbContextFactory<PermissionsDbContext>
    {
        public PermissionsDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<PermissionsDbContext>();
            var connectionString = "Server=localhost\\SQLEXPRESS;Database=N5.Test;User Id=sa;Password=123456;Encrypt=True;TrustServerCertificate=True;";

            optionsBuilder.UseSqlServer(connectionString);

            return new PermissionsDbContext(optionsBuilder.Options);
        }
    }
}
