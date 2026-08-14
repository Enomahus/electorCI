using System.Diagnostics.CodeAnalysis;
using Application.Api;
using Application.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Tools.Exceptions.Errors;

namespace Application.Features.RegistrationRequests.CreateRegistrationRequest
{
    [ExcludeFromCodeCoverage]
    [ApiController]
    [Route("registration-requests")]
    [OpenApiTag("registration-requests")]
    public class CreateRegistrationRequestController : ApiControllerBase
    {
        /// <summary>
        /// Create a new registration request
        /// </summary>
        /// <param name="formData"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost()]
        [OpenApiOperation("CreateRegistrationRequest", "Enregistre une nouvelle demande d'enrôlement.", "")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Result<Guid>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(Result<Error>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(Result<Error>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(Result<Error>))]
        public async Task<IActionResult> CreateRegistrationRequestAsync(
            [FromForm] CreateRegistrationRequestFromData formData,
            CancellationToken token
        )
        {
            var command = new CreateRegistrationRequestCommand()
            {
                RegistrationRequest = formData.GetRegistrationRequestData(),
                IdentityDocument = formData.IdentityDocument,
                ResidenceCertificate = formData.ResidenceCertificate,
                Photo = formData.Photo,
            };

            var result = await Mediator.Send(command, token);

            return StatusCode(StatusCodes.Status201Created, result);
        }
    }
}
