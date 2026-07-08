using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Application.Api;
using Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Tools.Exceptions.Errors;

namespace Application.Features.Security.ResetPassword
{
    [ExcludeFromCodeCoverage]
    [ApiController]
    [Route("auth")]
    [OpenApiTag("auth")]
    public class ResetPasswordController : ApiControllerBase
    {
        /// <summary>
        /// Reset the password
        /// </summary>
        /// <param name="command"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPut("reset-password")]
        [AllowAnonymous]
        [OpenApiOperation("ResetPassword", "Réinitialiser le mot de passe", "")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Result))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(Result<Error>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(Result<Error>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(Result<Error>))]
        public Task<Result> ResetPassword(
            [FromBody] ResetPasswordCommand command,
            CancellationToken token
        )
        {
            return Mediator.Send(command, token);
        }
    }
}
