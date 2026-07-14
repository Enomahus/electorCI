using Application.Api;
using Application.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using System.Diagnostics.CodeAnalysis;
using Tools.Exceptions.Errors;

namespace Application.Features.Districts.CreateDistrict
{
    [ExcludeFromCodeCoverage]
    [ApiController]
    [Route("districts")]
    [OpenApiTag("districts")]
    public class CreateDistrictController : ApiControllerBase
    {

        /// <summary>
        /// Create a new district
        /// </summary>
        /// <param name="command"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost()]
        [OpenApiOperation("CreateDistrict", "Enregistre une nouvelle Circonscription.", "")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Result<long>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(Result<Error>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(Result<Error>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(Result<Error>))]
        public async Task<IActionResult> CreateDistrictAsync(
            [FromBody] CreateDistrictCommand command,
            CancellationToken token
        )
        {
            var result = await Mediator.Send(command, token);
            return new ObjectResult(result) { StatusCode = StatusCodes.Status201Created };
        }
    }
}
