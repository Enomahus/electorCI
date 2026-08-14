using System.Diagnostics.CodeAnalysis;
using Application.Api;
using Application.Features.Common.GridData;
using Application.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Tools.Exceptions.Errors;

namespace Application.Features.RegistrationRequests.GetRegistrationRequestsForManagement
{
    [ExcludeFromCodeCoverage]
    [ApiController]
    [Route("registration-requests")]
    [OpenApiTag("registration-requests")]
    public class GetRegistrationRequestsForManagementController : ApiControllerBase
    {
        /// <summary>
        /// Get all registrations requests for management
        /// </summary>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("for-management/get-registration-requests")]
        [OpenApiOperation(
            "GetRegistrationRequestsForManagement",
            "Récupère toutes les demandes d'enregistrements pour les gestionnaires.",
            ""
        )]
        [ProducesResponseType(
            StatusCodes.Status200OK,
            Type = typeof(Result<GridDataResponse<GetRegistrationRequestsForManagementResponse>>)
        )]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(Result<Error>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(Result<Error>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(Result<Error>))]
        public Task<
            Result<GridDataResponse<GetRegistrationRequestsForManagementResponse>>
        > GetRegistrationRequestsForManagementAsync(
            [FromBody] GetRegistrationRequestsForManagementQuery query,
            CancellationToken cancellationToken
        )
        {
            return Mediator.Send(query, cancellationToken);
        }
    }
}
