using Microsoft.EntityFrameworkCore;
using N5.Permissions.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N5.Permissions.Infrastructure.Persistence
{
    public class PermissionsDbContext : DbContext
    {
        public PermissionsDbContext(DbContextOptions<PermissionsDbContext> options) : base(options)
        {
        }
        public DbSet<Permission> Permissions => Set<Permission>();
        public DbSet<PermissionType> PermissionTypes => Set<PermissionType>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Permission>()
                .HasOne(p => p.PermissionTypeNavigation)
                .WithMany(pt => pt.Permissions)
                .HasForeignKey(p => p.PermissionTypeId);
        }
    }
}
