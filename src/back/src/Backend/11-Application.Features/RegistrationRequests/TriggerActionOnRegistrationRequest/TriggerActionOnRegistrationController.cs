using System.Diagnostics.CodeAnalysis;
using Application.Api;
using Application.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Tools.Exceptions.Errors;

namespace Application.Features.RegistrationRequests.TriggerActionOnRegistrationRequest
{
    [ExcludeFromCodeCoverage]
    [ApiController]
    [Route("registration-requests")]
    [OpenApiTag("registration-requests")]
    public class TriggerActionOnRegistrationController : ApiControllerBase
    {
        /// <summary>
        /// Update a registrationRequest status (validation)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPut("{Id}/trigger-action")]
        [OpenApiOperation("TriggerActionOnRegistrationRequest", "Met à jour le status d'une demande.", "")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Result<Guid>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(Result<Error>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(Result<Error>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(Result<Error>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(Result<Error>))]
        public Task<Result<Guid>> TriggerActionOnRegistrationRequest(
            [FromRoute] Guid id,
            [FromBody] TriggerActionOnRegistrationRequestCommand command,
            CancellationToken cancellationToken
        )
        {
            command.RequestId = id;
            return Mediator.Send(command, cancellationToken);
        }
    }
}
