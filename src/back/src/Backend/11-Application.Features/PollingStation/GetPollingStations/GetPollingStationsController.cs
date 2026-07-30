using Application.Api;
using Application.Features.Common.GridData;
using Application.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using System.Diagnostics.CodeAnalysis;
using Tools.Exceptions.Errors;

namespace Application.Features.PollingStation.GetPollingStations
{
    [ExcludeFromCodeCoverage]
    [ApiController]
    [Route("polling-station")]
    [OpenApiTag("polling-station")]
    public class GetPollingStationsController : ApiControllerBase
    {
        /// <summary>
        /// Get all polling stations
        /// </summary>
        /// <param name="query">Query parameters for pagination and sorting</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>List of polling stations</returns>
        [HttpPost("get-polling-stations")]
        [OpenApiOperation("GetPollingStations", "Récupère tous les Bureaux de Vote.", "")]
        [ProducesResponseType(
            StatusCodes.Status200OK,
            Type = typeof(Result<GridDataResponse<GetPollingStationsResponse>>)
        )]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(Result<Error>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(Result<Error>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(Result<Error>))]
        public async Task<Result<GridDataResponse<GetPollingStationsResponse>>> GetPollingStationsAsync(
            [FromBody] GetPollingStationsQuery query,
            CancellationToken token
        )
        {
            return await Mediator.Send(query, token);
        }
    }
}
