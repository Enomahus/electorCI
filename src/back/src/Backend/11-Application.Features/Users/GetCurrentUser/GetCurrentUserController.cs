using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Application.Api;
using Application.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Tools.Exceptions.Errors;

namespace Application.Features.Users.GetCurrentUser
{
    [ExcludeFromCodeCoverage]
    [ApiController]
    [Route("user")]
    [OpenApiTag("user")]
    public class GetCurrentUserController : ApiControllerBase
    {
        /// <summary>
        /// Get the logged in user
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("current")]
        [OpenApiOperation(
            "GetCurrentUser",
            "Récupère les informations de l'utilisateur connecté.",
            ""
        )]
        [ProducesResponseType(
            StatusCodes.Status200OK,
            Type = typeof(Result<GetCurrentUserResponse>)
        )]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(Result<Error>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(Result<Error>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(Result<Error>))]
        public async Task<IActionResult> GetCurrentUserAsync(CancellationToken cancellationToken)
        {
            var query = new GetCurrentUserQuery();
            var result = await Mediator.Send(query, cancellationToken);

            return Ok(result);
        }
    }
}
