using N5.Permissions.Application.Common.Models;
using N5.Permissions.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N5.Permissions.Application.Common.Interfaces
{
    public interface IPermissionIndexer
    {
        Task IndexAsync(PermissionDocument document, CancellationToken cancellationToken);
    }
}
