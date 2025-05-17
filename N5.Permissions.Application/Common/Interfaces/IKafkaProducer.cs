using N5.Permissions.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N5.Permissions.Application.Common.Interfaces
{
    public interface IKafkaProducer
    {
        Task ProduceAsync(PermissionEvent permissionEvent, CancellationToken cancellationToken);
    }
}

