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

namespace Application.Features.Document.GetDocumentsInfos
{
    [ExcludeFromCodeCoverage]
    [ApiController]
    [Route("documents")]
    [OpenApiTag("documents")]
    public class GetDocumentsInfosController : ApiControllerBase
    {
        /// <summary>
        /// Gets file information
        /// </summary>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("")]
        [OpenApiOperation("GetDocumentsInfos", "Récupère les informations des documents.", "")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Result<GetDocumentsInfosResponse>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(Result<Error>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(Result<Error>))]
        public Task<Result<GetDocumentsInfosResponse>> GetDocumentsInfosAsync(
            [FromBody] GetDocumentsInfosQuery query,
            CancellationToken cancellationToken
        )
        {
            return Mediator.Send(query, cancellationToken);
        }
    }
}
