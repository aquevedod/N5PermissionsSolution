using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N5.Permissions.Application.Common.Models
{
    public class PermissionEvent
    {
        public Guid Id { get; set; }
        public string Operation { get; set; } = string.Empty;
    }
}

