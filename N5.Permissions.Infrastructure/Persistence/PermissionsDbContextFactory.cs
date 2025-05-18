using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using N5.Permissions.Infrastructure.Persistence;

namespace N5.Permissions.Infrastructure
{
	public class PermissionsDbContextFactory : IDesignTimeDbContextFactory<PermissionsDbContext>
	{
		public PermissionsDbContext CreateDbContext(string[] args)
		{
			var optionsBuilder = new DbContextOptionsBuilder<PermissionsDbContext>();

			// ⚠️ Reemplazá esto por tu cadena de conexión real o temporal
			var connectionString = "Server=localhost\\SQLEXPRESS;Database=master;Trusted_Connection=True;TrustServerCertificate=True";

			optionsBuilder.UseSqlServer(connectionString);

			return new PermissionsDbContext(optionsBuilder.Options);
		}
	}
}
