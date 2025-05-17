using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N5.Permissions.Domain.Entities
{
    public class PermissionType
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;

        public ICollection<Permission>? Permissions { get; set; } = new List<Permission>();
    }
}
