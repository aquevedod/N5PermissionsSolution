using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N5.Permissions.Application.Commands
{
    public class ModifyPermissionCommand : IRequest<bool>
    {
        public int Id { get; set; }
        public string EmployeeForename { get; set; } = string.Empty;

        public string EmployeeSurname { get; set; } = string.Empty;

        public int PermissionTypeId { get; set; }

        public DateTime PermissionDate { get; set; }

    }
}
