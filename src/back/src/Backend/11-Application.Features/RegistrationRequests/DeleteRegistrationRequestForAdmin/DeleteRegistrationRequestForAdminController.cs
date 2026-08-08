using System.Diagnostics.CodeAnalysis;
using Application.Api;
using Application.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Tools.Exceptions.Errors;

namespace Application.Features.RegistrationRequests.DeleteRegistrationRequestForAdmin
{
    [ExcludeFromCodeCoverage]
    [ApiController]
    [Route("registration-requests")]
    [OpenApiTag("registration-requests")]
    public class DeleteRegistrationRequestForAdminController : ApiControllerBase
    {
        /// <summary>
        /// Delete a registration request for admin
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpDelete("for-admin/{id}")]
        [OpenApiOperation(
            "DeleteRegistrationeRequestForAdmin",
            "Supprime une demande d'enrôlement en tant qu'administrateur.",
            ""
        )]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Result))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(Result<Error>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(Result<Error>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(Result<Error>))]
        public Task<Result> DeleteRegistrationeRequestForAdminAsync(
            Guid id,
            CancellationToken cancellationToken
        )
        {
            var command = new DeleteRegistrationRequestForAdminCommand(id);
            return Mediator.Send(command, cancellationToken);
        }
    }
}
