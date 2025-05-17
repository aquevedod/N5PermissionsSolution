using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N5.Permissions.Domain.DTO
{
    public class RequestPermissionCommand : IRequest<int>
    {
        public string EmployeeForename { get; set; } = string.Empty;
        public string EmployeeSurname { get; set; } = string.Empty;
        public int PermissionTypeId { get; set; }

        public RequestPermissionCommand(
            string employeeForename,
            string employeeSurname,
            int permissionTypeId)
        {
            EmployeeForename = employeeForename;
            EmployeeSurname = employeeSurname;
            PermissionTypeId = permissionTypeId;
        }
    }
}
