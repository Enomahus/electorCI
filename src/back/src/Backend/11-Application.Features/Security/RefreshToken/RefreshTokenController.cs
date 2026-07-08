using System.Diagnostics.CodeAnalysis;
using Application.Api;
using Application.Features.Security.Common;
using Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Tools.Exceptions.Errors;

namespace Application.Features.Security.RefreshToken
{
    [ExcludeFromCodeCoverage]
    [ApiController]
    [Route("auth")]
    [OpenApiTag("auth")]
    public class RefreshTokenController : ApiControllerBase
    {
        /// <summary>
        /// Get a new access token with a refresh token
        /// </summary>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("refreshtoken")]
        [AllowAnonymous]
        [OpenApiOperation("RefreshToken", "Renouvelle un access token.", "")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Result<TokenResponse>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(Result<Error>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(Result<Error>))]
        public Task<Result<TokenResponse>> RefreshToken(
            [FromBody] RefreshTokenCommand query,
            CancellationToken cancellationToken
        )
        {
            return Mediator.Send(query, cancellationToken);
        }
    }
}
