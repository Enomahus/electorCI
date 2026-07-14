using System.Diagnostics.CodeAnalysis;
using Application.Api;
using Application.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using NSwag.Annotations;
using Tools.Exceptions.Errors;

namespace Application.Features.Users.UpdateCurrentUser
{
    [ExcludeFromCodeCoverage]
    [ApiController]
    [Route("user")]
    [OpenApiTag("user")]
    public class UpdateCurrentUserController : ApiControllerBase
    {
        /// <summary>
        /// Update the current user
        /// </summary>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPut("my-information")]
        [OpenApiOperation("UpdateCurrentUser", "Met à jour les informations de l'utilisateur courant.", "")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Result<Guid>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(Result<Error>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(Result<Error>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(Result<Error>))]
        public Task<Result<Guid>> UpdateCurrentUserAsync([FromBody] UpdateCurrentUserCommand command, CancellationToken cancellationToken)
        {
            return Mediator.Send(command, cancellationToken);
        }
    }
}
