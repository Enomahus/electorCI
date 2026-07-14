using Application.Api;
using Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using System.Diagnostics.CodeAnalysis;
using Tools.Exceptions.Errors;

namespace Application.Features.Users.RegisterUser
{
    [ExcludeFromCodeCoverage]
    [ApiController]
    [Route("user")]
    [OpenApiTag("user")]
    public class RegisterUserController : ApiControllerBase
    {
        /// <summary>
        /// Create a user
        /// </summary>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("register")]
        [OpenApiOperation("RegisterUser", "Permet à un utilisateur de s'enregistrer.", "")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Result<Guid>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(Result<Error>))]
        public async Task<IActionResult> RegisterUserAsync(
            RegisterUserCommand command,
            CancellationToken cancellationToken
        )
        {
            var result = await Mediator.Send(command, cancellationToken);
            return new ObjectResult(result) { StatusCode = StatusCodes.Status201Created };
        }
    }
}
