using Application.Api;
using Application.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using System.Diagnostics.CodeAnalysis;
using Tools.Exceptions.Errors;

namespace Application.Features.Districts.ToggleActiveDistrict
{
    [ExcludeFromCodeCoverage]
    [ApiController]
    [Route("districts")]
    [OpenApiTag("districts")]
    public class ToggleActiveDistrictController : ApiControllerBase
    {
        [HttpPut("toggle-active-district")]
        [OpenApiOperation("ToggleActiveDistrict", "Active ou désactive une circonscription.", "")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Result))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(Result<Error>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(Result<Error>))]
        public async Task<Result> ToggleActiveDistrictAsync([FromBody] ToggleActiveDistrictCommand cmd, CancellationToken token)
        {
            return await Mediator.Send(cmd, token);
        }
    }
}
