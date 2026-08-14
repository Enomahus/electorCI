using System.Diagnostics.CodeAnalysis;
using Application.Api;
using Application.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Tools.Exceptions.Errors;

namespace Application.Features.RegistrationRequests.GetRegistrationRequest
{
    [ExcludeFromCodeCoverage]
    [ApiController]
    [Route("registration-requests")]
    [OpenApiTag("registration-requests")]
    public class GetRegistrationRequestController : ApiControllerBase
    {
        /// <summary>
        /// Get registration request
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [OpenApiOperation("GetRegistrationRequest", "Récupère une demande d'enrôlement.", "")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Result<GetRegistrationRequestResponse>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(Result<Error>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(Result<Error>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(Result<Error>))]
        public Task<Result<GetRegistrationRequestResponse>> GetRegistrationRequestAsync(
            [FromRoute] Guid id,
            CancellationToken cancellationToken
        )
        {
            var query = new GetRegistrationRequestQuery(id);
            return Mediator.Send(query, cancellationToken);
        }
    }
}
