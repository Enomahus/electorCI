using System.Diagnostics.CodeAnalysis;
using Application.Api;
using Application.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Tools.Exceptions.Errors;

namespace Application.Features.Citizens.CreateCitizen
{
    [ExcludeFromCodeCoverage]
    [ApiController]
    [Route("citizens")]
    [OpenApiTag("citizens")]
    public class CreateCitizenController : ApiControllerBase
    {
        /// <summary>
        /// Create a new citizen
        /// </summary>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost()]
        [OpenApiOperation("CreateCitizen", "Créer un citoyen.", "")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Result<Guid>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(Result<Error>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(Result<Error>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(Result<Error>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(Result<Error>))]
        public async Task<IActionResult> CreateCitizenAsync(
            [FromBody] CreateCitizenCommand command,
            CancellationToken cancellationToken
        )
        {
            var result = await Mediator.Send(command, cancellationToken);
            return new ObjectResult(result) { StatusCode = StatusCodes.Status201Created };
        }
    }
}
