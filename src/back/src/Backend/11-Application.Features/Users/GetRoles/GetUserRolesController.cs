using Application.Api;
using Application.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Tools.Exceptions.Errors;

namespace Application.Features.Users.GetRoles
{
    [ExcludeFromCodeCoverage]
    [ApiController]
    [Route("user")]
    [OpenApiTag("user")]
    public class GetUserRolesController : ApiControllerBase
    {
        /// <summary>
        /// Get the roles
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet()]
        [Route("roles", Name = "GetUserRoles")]
        [OpenApiOperation("GetUserRoles", "Récupère tous les rôles possibles pour un utilisateur.", "")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Result<List<RoleModel>>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(Result<Error>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(Result<Error>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(Result<Error>))]
        public Task<Result<List<RoleModel>>> GetUserRoles(CancellationToken cancellationToken)
        {
            var query = new GetUserRolesQuery() { };
            return Mediator.Send(query, cancellationToken);
        }
    }
}
