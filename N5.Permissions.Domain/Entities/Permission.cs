using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N5.Permissions.Domain.Entities
{
    public class Permission
    {
        public int Id { get; set; }
        public string EmployeeForename { get; set; } = string.Empty;
        public string EmployeeSurname { get; set; } = string.Empty;

        /// <summary>
        /// Foreign key to the PermissionType table.
        /// </summary>
        public int PermissionTypeId { get; set; }
        public DateTime PermissionDate { get; set; }

        /// <summary>
        /// Navigation property to the PermissionType table.
        /// </summary>
        public PermissionType? PermissionTypeNavigation { get; set; }
    }
}
