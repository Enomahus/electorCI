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

namespace Application.Features.Users.UpdateUser
{
    [ExcludeFromCodeCoverage]
    [ApiController]
    [Route("user")]
    [OpenApiTag("user")]
    public class UpdateUserController : ApiControllerBase
    {
        /// <summary>
        /// Update a user
        /// </summary>
        /// <param name="id"></param>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [OpenApiOperation("UpdateUser", "Met à jour un utilisateur.", "")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Result<Guid>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(Result<Error>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(Result<Error>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(Result<Error>))]
        public Task<Result<Guid>> UpdateUserAsync([FromRoute] Guid id, [FromBody] UpdateUserCommand command, CancellationToken cancellationToken)
        {
            command.UserId = id;
            return Mediator.Send(command, cancellationToken);
        }
    }
}
