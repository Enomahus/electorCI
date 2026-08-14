using System.Diagnostics.CodeAnalysis;
using Application.Api;
using Application.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Tools.Exceptions.Errors;

namespace Application.Features.Districts.GetDistricts
{
    [ExcludeFromCodeCoverage]
    [ApiController]
    [Route("districts")]
    [OpenApiTag("districts")]
    public class GetDistrictsController : ApiControllerBase
    {
        /// <summary>
        /// Get districts
        /// </summary>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("get-districts")]
        [OpenApiOperation("GetDistricts", "Récupère les circonscriptions.", "")]
        [ProducesResponseType(
            StatusCodes.Status200OK,
            Type = typeof(Result<IEnumerable<GetDistrictsResponse>>)
        )]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(Result<Error>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(Result<Error>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(Result<Error>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(Result<Error>))]
        public Task<Result<IEnumerable<GetDistrictsResponse>>> GetDistrictsAsync(
            [FromBody] GetDistrictsQuery query,
            CancellationToken cancellationToken
        )
        {
            return Mediator.Send(query, cancellationToken);
        }
    }
}
