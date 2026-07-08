using System;
using System.Collections.Generic;
using System.Text;
using Application.Common.Enums;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.PeopleService.v1;
using Infrastructure.ExternalAuth.Configuration;
using Infrastructure.ExternalAuth.Interfaces;
using Infrastructure.ExternalAuth.Models;
using Microsoft.Extensions.Options;
using Tools.Configuration;
using Tools.Exceptions;

namespace Infrastructure.ExternalAuth.Services
{
    public class GoogleAuthService : IExternalAuthService
    {
        private readonly GoogleAuthorizationCodeFlow.Initializer _codeFlowInit;
        private readonly string _redirectUri;

        public GoogleAuthService(
            IOptionsFactory<ExternalAuthConfiguration> configFactory,
            IOptions<AppConfiguration> appConfig
        )
        {
            var config = configFactory.Create(ExternalAuthServiceKeys.GoogleConfiguration);
            if (config.ClientSecret is null)
            {
                throw new ConfigurationMissingException(
                    "Missing configuration : GoogleAuth.ClientSecret"
                );
            }
            if (config.ClientId is null)
            {
                throw new ConfigurationMissingException(
                    "Missing configuration : GoogleAuth.ClientId"
                );
            }
            if (appConfig.Value.AppUrl is null)
            {
                throw new ConfigurationMissingException("Missing configuration : AppConfig.AppUrl");
            }
            var clientSecret = new ClientSecrets()
            {
                ClientId = config.ClientId,
                ClientSecret = config.ClientSecret,
            };
            _codeFlowInit = new GoogleAuthorizationCodeFlow.Initializer()
            {
                ClientSecrets = clientSecret,
                Scopes =
                [
                    "https://www.googleapis.com/auth/userinfo.email",
                    "https://www.googleapis.com/auth/userinfo.profile",
                ],
            };

            _redirectUri = $"{appConfig.Value.AppUrl}/login/google";
        }

        public async Task<ExternallyAuthenticatedPersonModel> CheckAuthorizationCodeAsync(
            string code,
            CancellationToken cancellationToken
        )
        {
            var tokenResponse = await GetTokenAsync(code, cancellationToken);

            var peopleService = new PeopleServiceService();
            var request = peopleService.People.Get(
                "people/me?personFields=names,emailAddresses,phoneNumbers"
            );
            request.AccessToken = tokenResponse.AccessToken;

            var result = await request.ExecuteAsync(cancellationToken);

            var model = new ExternallyAuthenticatedPersonModel()
            {
                AuthProvider = AuthProvider.Google,
                FirstName = result.Names?[0]?.GivenName,
                LastName = result.Names?[0]?.FamilyName,
                Email = result.EmailAddresses?[0]?.Value,
                Phone = result.PhoneNumbers?[0]?.Value,
            };

            return model;
        }

        private Task<TokenResponse> GetTokenAsync(string code, CancellationToken cancellationToken)
        {
            var flow = new GoogleAuthorizationCodeFlow(_codeFlowInit);

            return flow.ExchangeCodeForTokenAsync(null, code, _redirectUri, cancellationToken);
        }
    }
}
