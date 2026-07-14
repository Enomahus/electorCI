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

namespace Application.Features.Users.DeleteUser
{
    [ExcludeFromCodeCoverage]
    [ApiController]
    [Route("user")]
    [OpenApiTag("user")]
    public class DeleteUserController : ApiControllerBase
    {
        /// <summary>
        /// Delete a user
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [OpenApiOperation("DeleteUser", "Supprime un utilisateur.", "")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Result))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(Result<Error>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(Result<Error>))]
        public Task<Result> DeleteUserAsync(Guid id, CancellationToken cancellationToken)
        {
            var command = new DeleteUserCommand() { Id = id };
            return Mediator.Send(command, cancellationToken);
        }
    }
}
