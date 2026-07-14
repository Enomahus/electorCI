using Application.Api;
using Application.Features.Common.DataGrid;
using Application.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
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
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Result<DataGridResponse<GetUsersResponse>>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(Result<Error>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(Result<Error>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(Result<Error>))]
        public Task<Result<DataGridResponse<GetUsersResponse>>> GetUsers(
            [FromBody] GetUsersQuery query,
            CancellationToken cancellationToken
        )
        {
            return Mediator.Send(query, cancellationToken);
        }
    }
}
