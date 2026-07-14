using System.Diagnostics.CodeAnalysis;
using Application.Api;
using Application.Features.Common.GridData;
using Application.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Tools.Exceptions.Errors;

namespace Application.Features.Users.GetUsers
{
    [ExcludeFromCodeCoverage]
    [ApiController]
    [Route("users")]
    [OpenApiTag("users")]
    public class GetUsersController : ApiControllerBase
    {
        /// <summary>
        /// Get the users
        /// </summary>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("get-users")]
        [OpenApiOperation("GetUsers", "Récupère les utilisateurs.", "")]
        [ProducesResponseType(
            StatusCodes.Status200OK,
            Type = typeof(Result<GridDataResponse<GetUsersResponse>>)
        )]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(Result<Error>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(Result<Error>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(Result<Error>))]
        public Task<Result<GridDataResponse<GetUsersResponse>>> GetUsers(
            [FromBody] GetUsersQuery query,
            CancellationToken cancellationToken
        )
        {
            return Mediator.Send(query, cancellationToken);
        }
    }
}
