using System.Diagnostics.CodeAnalysis;
using Application.Api;
using Application.Features.Common.GridData;
using Application.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Tools.Exceptions.Errors;

namespace Application.Features.RegistrationRequests.GetRegistrationRequestsForAdmin
{
    [ExcludeFromCodeCoverage]
    [ApiController]
    [Route("registration-requests")]
    [OpenApiTag("registration-requests")]
    public class GetRegistrationRequestsForAdminController : ApiControllerBase
    {
        /// <summary>
        /// Get all registration request for admin
        /// </summary>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("for-admin/get-registration-requests")]
        [OpenApiOperation(
            "GetRegistrationRequestsForAdmin",
            "Récupère toutes les demandes d'enregistrements pour les admins.",
            ""
        )]
        [ProducesResponseType(
            StatusCodes.Status200OK,
            Type = typeof(Result<GridDataResponse<GetRegistrationRequestsForAdminResponse>>)
        )]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(Result<Error>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(Result<Error>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(Result<Error>))]
        public Task<
            Result<GridDataResponse<GetRegistrationRequestsForAdminResponse>>
        > GetRegistrationRequestsForAdminAsync(
            [FromBody] GetRegistrationRequestsForAdminQuery query,
            CancellationToken cancellationToken
        )
        {
            return Mediator.Send(query, cancellationToken);
        }
    }
}
