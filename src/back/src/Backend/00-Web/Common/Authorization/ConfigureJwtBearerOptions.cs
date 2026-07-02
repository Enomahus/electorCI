using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Web.Common.Authorization
{
    public class ConfigureJwtBearerOptions : IPostConfigureOptions<JwtBearerOptions>
    {
        private readonly TimeProvider _timeProvider;

        public ConfigureJwtBearerOptions(TimeProvider timeProvider)
        {
            _timeProvider = timeProvider;
        }

        public void PostConfigure(string? name, JwtBearerOptions options)
        {
            if (name == null || name == JwtBearerDefaults.AuthenticationScheme)
            {
                options.TokenValidationParameters.LifetimeValidator = (
                    DateTime? notBefore,
                    DateTime? expires,
                    SecurityToken securityToken,
                    TokenValidationParameters validationParameters
                ) =>
                {
                    var now = _timeProvider.GetUtcNow();
                    return (notBefore is null || notBefore <= now)
                        && (expires is null || expires > now);
                };
            }
        }
    }
}
