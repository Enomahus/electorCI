using System.Diagnostics.CodeAnalysis;
using Application.Api;
using Application.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Tools.Exceptions.Errors;

namespace Application.Features.Citizens.GetCitizens
{
    [ExcludeFromCodeCoverage]
    [ApiController]
    [Route("citizens")]
    [OpenApiTag("citizens")]
    public class GetCitizensController : ApiControllerBase
    {
        /// <summary>
        /// Get citizens
        /// </summary>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("get-citizens")]
        [OpenApiOperation("GetCitizens", "Récupère les citoyens.", "")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Result<List<GetCitizensResponse>>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(Result<Error>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(Result<Error>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(Result<Error>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(Result<Error>))]
        public Task<Result<List<GetCitizensResponse>>> GetCitizensAsync(
            [FromBody] GetCitizensQuery query,
            CancellationToken cancellationToken
        )
        {
            return Mediator.Send(query, cancellationToken);
        }
    }
}
