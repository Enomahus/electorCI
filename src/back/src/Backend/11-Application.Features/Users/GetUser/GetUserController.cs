using Application.Api;
using Application.Features.Users.Common;
using Application.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Tools.Exceptions.Errors;

namespace Application.Features.Users.GetUser
{
    [ExcludeFromCodeCoverage]
    [ApiController]
    [Route("user")]
    [OpenApiTag("user")]
    public class GetUserController : ApiControllerBase
    {
        /// <summary>
        /// Get the users
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet()]
        [Route("{id}", Name = "GetUser")]
        [OpenApiOperation("GetUser", "Récupère un utilisateur.", "")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Result<UserModel>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(Result<Error>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(Result<Error>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(Result<Error>))]
        public Task<Result<UserModel>> GetUser(Guid id, CancellationToken cancellationToken)
        {
            var query = new GetUserQuery() { UserId = id };
            return Mediator.Send(query, cancellationToken);
        }
    }
}
