using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N5.Permissions.Application.DTO
{
    public class CreatePermissionTypeCommand : IRequest<int>
    {
        public string Description { get; set; } = string.Empty;
    }
}
