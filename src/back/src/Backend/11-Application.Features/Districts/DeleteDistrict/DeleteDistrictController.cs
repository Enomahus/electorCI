using System.Diagnostics.CodeAnalysis;
using Application.Api;
using Application.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Tools.Exceptions.Errors;

namespace Application.Features.Districts.DeleteDistrict
{
    [ExcludeFromCodeCoverage]
    [ApiController]
    [Route("districts")]
    [OpenApiTag("districts")]
    public class DeleteDistrictController : ApiControllerBase
    {
        /// <summary>
        /// Delete a district
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [OpenApiOperation("DeleteDistrict", "Supprime une circonscription.", "")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Result))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(Result<Error>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(Result<Error>))]
        public Task<Result> DeleteDistrictAsync(long id, CancellationToken cancellationToken)
        {
            var command = new DeleteDistrictCommand(id);
            return Mediator.Send(command, cancellationToken);
        }
    }
}
