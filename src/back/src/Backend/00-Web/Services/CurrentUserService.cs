using System.Security.Claims;
using Application.Common.Interfaces.Services;
using Microsoft.AspNetCore.Authentication;
using Pcea.Core.Net.Authorization.Application.Interfaces.Services;
using Pcea.Core.Net.Authorization.Persistence.Interfaces.Services;
using Pcea.Core.Net.Authorization.Web.Interfaces.Services;
using Pcea.Core.Net.Authorization.Web.Services;

namespace Web.Services
{
    public class CurrentUserService(
        IHttpContextAccessor httpContextAccessor,
        ITokenRoleClaimBuilder<long> tokenRoleClaimBuilder,
        IPermissionsService<Guid> permissionsService
    )
        : CurrentUserTokenService<Guid, long>(
            httpContextAccessor,
            tokenRoleClaimBuilder,
            permissionsService
        ),
            ICurrentUserService,
            ICurrentUserPermissionsProvider,
            ICurrentUserEntityPermissionsProvider<long>
    {
        private readonly IHttpContextAccessor httpContextAccessor = httpContextAccessor;

        public Guid? UserId =>
            Guid.TryParse(
                httpContextAccessor
                    .HttpContext?.User.Claims.FirstOrDefault(c =>
                        c.Type == ClaimTypes.NameIdentifier
                    )
                    ?.Value
                    ?? "",
                out var parsedId
            )
                ? parsedId
                : null;

        public string? UserEmail =>
            httpContextAccessor
                .HttpContext?.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)
                ?.Value;

        public string? ClientIp =>
            httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "";

        public string? LanguageCode =>
            httpContextAccessor
                .HttpContext?.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Locality)
                ?.Value;

        public Task<string?> GetTokenAsync(CancellationToken token = default)
        {
            return httpContextAccessor.HttpContext?.GetTokenAsync("access_token")
                ?? Task.FromResult<string?>(null);
        }

        public override Task<bool> IsCurrentUserAuthenticatedAsync(
            CancellationToken cancellationToken = default
        )
        {
            return Task.FromResult(UserId.HasValue);
        }

        protected override Guid ParseRoleId(string roleIdStr)
        {
            return Guid.Parse(roleIdStr);
        }
    }
}
