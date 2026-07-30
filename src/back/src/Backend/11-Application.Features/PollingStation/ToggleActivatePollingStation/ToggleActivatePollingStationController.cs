using Application.Api;
using Application.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using System.Diagnostics.CodeAnalysis;
using Tools.Exceptions.Errors;

namespace Application.Features.PollingStation.ToggleActivatePollingStation
{
    [ExcludeFromCodeCoverage]
    [ApiController]
    [Route("polling-station")]
    [OpenApiTag("polling-station")]
    public class ToggleActivatePollingStationController : ApiControllerBase
    {
        /// <summary>
        /// Disable or enable a polling station
        /// </summary>
        /// <param name="command">The update data</param>
        /// <param name="token">Cancellation token</param>
        /// <returns></returns>
        [HttpPut("toggle-activate-polling-station")]
        [OpenApiOperation("ToggleActivatePollingStation", "Active ou désactive un bureau de vote.", "")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Result))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(Result<Error>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(Result<Error>))]
        public async Task<Result> ToggleActivatePollingStationAsync([FromBody] ToggleActivatePollingStationCommand command, CancellationToken token)
        {
            return await Mediator.Send(command, token);
        }
    }
}
