using MediatR;
using N5.Permissions.Application.DTO;
using N5.Permissions.Domain.Entities;

namespace N5.Permissions.Application.Queries
{
    public class GetPermissionsQuery : IRequest<List<PermissionDto>>
    {
    }
}
