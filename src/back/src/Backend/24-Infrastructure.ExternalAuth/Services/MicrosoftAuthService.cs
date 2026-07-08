using Application.Common.Enums;
using Infrastructure.ExternalAuth.Configuration;
using Infrastructure.ExternalAuth.Interfaces;
using Infrastructure.ExternalAuth.Models;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Microsoft.Kiota.Abstractions.Authentication;
using Tools.Configuration;
using Tools.Exceptions;

namespace Infrastructure.ExternalAuth.Services
{
    public class MicrosoftAuthService : IExternalAuthService
    {
        private readonly ConfidentialClientApplicationBuilder _clientAppBuilder;

        public MicrosoftAuthService(
            IOptionsFactory<ExternalAuthConfiguration> configFactory,
            IOptions<AppConfiguration> appConfig
        )
        {
            var config = configFactory.Create(ExternalAuthServiceKeys.MicrosoftConfiguration);
            if (config.ClientSecret is null)
            {
                throw new ConfigurationMissingException(
                    "Missing configuration : MicrosoftAuth.ClientSecret"
                );
            }
            if (config.ClientId is null)
            {
                throw new ConfigurationMissingException(
                    "Missing configuration : MicrosoftAuth.ClientId"
                );
            }
            if (appConfig.Value.AppUrl is null)
            {
                throw new ConfigurationMissingException("Missing configuration : AppConfig.AppUrl");
            }
            _clientAppBuilder = ConfidentialClientApplicationBuilder
                .Create(config.ClientId)
                .WithClientSecret(config.ClientSecret)
                .WithRedirectUri($"{appConfig.Value.AppUrl}/login/microsoft");
        }

        public async Task<ExternallyAuthenticatedPersonModel> CheckAuthorizationCodeAsync(
            string code,
            CancellationToken cancellationToken
        )
        {
            var tokenCred = new BaseBearerTokenAuthenticationProvider(
                new AuthorizationCodeTokenProvider(code, _clientAppBuilder)
            );
            var graphClient = new GraphServiceClient(tokenCred);

            var result = await graphClient.Me.GetAsync(
                config =>
                {
                    config.QueryParameters.Select =
                    [
                        "givenName",
                        "surname",
                        "mail",
                        "userPrincipalName",
                        "mobilePhone",
                    ];
                },
                cancellationToken
            );

            var email = result?.Mail ?? result?.UserPrincipalName;

            var model = new ExternallyAuthenticatedPersonModel()
            {
                AuthProvider = AuthProvider.Microsoft,
                FirstName = result?.GivenName,
                LastName = result?.Surname,
                Email = email,
                Phone = result?.MobilePhone,
            };

            return model;
        }

        private sealed class AuthorizationCodeTokenProvider : IAccessTokenProvider
        {
            private readonly string _code;
            private readonly ConfidentialClientApplicationBuilder _clientAppBuilder;

            private string? AccessToken;

            public AllowedHostsValidator AllowedHostsValidator { get; }

            public AuthorizationCodeTokenProvider(
                string code,
                ConfidentialClientApplicationBuilder clientAppBuilder
            )
            {
                AllowedHostsValidator = new AllowedHostsValidator();
                _code = code;
                _clientAppBuilder = clientAppBuilder;
            }

            public async Task<string> GetAuthorizationTokenAsync(
                Uri uri,
                Dictionary<string, object>? additionalAuthenticationContext = null,
                CancellationToken cancellationToken = default
            )
            {
                if (AccessToken is null)
                {
                    var clientApp = _clientAppBuilder.Build();
                    var authResult = await clientApp
                        .AcquireTokenByAuthorizationCode(
                            ["openid", "profile", "offline_access", "User.Read"],
                            _code
                        )
                        .ExecuteAsync(cancellationToken);

                    AccessToken = authResult.AccessToken;
                }

                return AccessToken;
            }
        }
    }
}
