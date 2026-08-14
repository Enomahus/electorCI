using System.Diagnostics.CodeAnalysis;
using Application.Api;
using Application.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Tools.Exceptions.Errors;

namespace Application.Features.RegistrationRequests.DeleteRegistrationRequest
{
    [ExcludeFromCodeCoverage]
    [ApiController]
    [Route("registration-requests")]
    [OpenApiTag("registration-requests")]
    public class DeleteRegistrationRequestController : ApiControllerBase
    {
        /// <summary>
        /// Delete a registration request
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [OpenApiOperation("DeleteRegistrationeRequest", "Supprime une demande d'enrôlement.", "")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Result))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(Result<Error>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(Result<Error>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(Result<Error>))]
        public Task<Result> DeleteRegistrationeRequestAsync(Guid id, CancellationToken cancellationToken)
        {
            var command = new DeleteRegistrationRequestCommand(id);
            return Mediator.Send(command, cancellationToken);
        }
    }
}
