using System.Diagnostics.CodeAnalysis;
using Application.Api;
using Application.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Tools.Exceptions.Errors;

namespace Application.Features.Districts.UpdateDistrict
{
    [ExcludeFromCodeCoverage]
    [ApiController]
    [Route("districts")]
    [OpenApiTag("districts")]
    public class UpdateDistrictController : ApiControllerBase
    {
        /// <summary>
        /// Update a district.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="Id"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPut()]
        [OpenApiOperation("UpdateDistrict", "Met à jour une Circonscrption.", "")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Result<long>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(Result<Error>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(Result<Error>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(Result<Error>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(Result<Error>))]
        public async Task<Result<long>> UpdateDistrictAsync(
            [FromBody] UpdateDistrictCommand command,
            [FromRoute] long Id,
            CancellationToken token
        )
        {
            command.Id = Id;
            return await Mediator.Send(command, token);
        }
    }
}
