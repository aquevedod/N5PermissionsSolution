using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N5.Permissions.Domain.Interfaces
{
    public interface IUnitOfWork
    {
        /// <summary>
        /// Saves all changes made in this context to the database.
        /// </summary>
        Task SaveChangesAsync(CancellationToken cancellationToken);
    }
}
