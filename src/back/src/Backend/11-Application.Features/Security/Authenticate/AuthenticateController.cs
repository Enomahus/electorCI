using System.Diagnostics.CodeAnalysis;
using Application.Api;
using Application.Features.Security.Common;
using Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Tools.Exceptions.Errors;

namespace Application.Features.Security.Authenticate
{
    [ExcludeFromCodeCoverage]
    [ApiController]
    [Route("auth")]
    [OpenApiTag("auth")]
    public class AuthenticateController : ApiControllerBase
    {
        /// <summary>
        /// Authenticate a user with an login and password
        /// </summary>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("token")]
        [AllowAnonymous]
        [OpenApiOperation("Authenticate", "Authentifie un utilisateur.", "")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Result<TokenResponse>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(Result<Error>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(Result<Error>))]
        public Task<Result<TokenResponse>> Authenticate(
            [FromBody] AuthenticateCommand query,
            CancellationToken cancellationToken
        )
        {
            return Mediator.Send(query, cancellationToken);
        }
    }
}
