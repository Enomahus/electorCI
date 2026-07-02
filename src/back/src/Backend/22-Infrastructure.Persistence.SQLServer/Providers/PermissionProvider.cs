using Infrastructure.Persistence.SQLServer.Contexts;
using Microsoft.EntityFrameworkCore;
using Pcea.Core.Net.Authorization.Persistence.Interfaces.Domain;
using Pcea.Core.Net.Authorization.Persistence.Interfaces.Services;

namespace Infrastructure.Persistence.SQLServer.Providers
{
    public class PermissionProvider(ReadOnlyDbContext context) : IPermissionsProvider<Guid>
    {
        public async Task<IEnumerable<IPermission>> FetchRolePermissionsAsync(
            IEnumerable<Guid> rolesId,
            CancellationToken token = default
        )
        {
            var rolesLst = rolesId.ToList();
            var permissions = await context
                .Roles.Where(r => rolesLst.Contains(r.Id))
                .SelectMany(r => r.Actions)
                .SelectMany(a => a.Permissions)
                .ToListAsync(token);

            return permissions;
        }
    }
}
