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

namespace Application.Features.Document.GetDocumentInfo
{
    [ExcludeFromCodeCoverage]
    [ApiController]
    [Route("documents")]
    [OpenApiTag("documents")]
    public class GetDocumentInfoController : ApiControllerBase
    {
        /// <summary>
        /// Gets file information
        /// </summary>
        /// <param name="documentId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("{documentId}")]
        [OpenApiOperation("GetDocumentInfo", "Récupère les informations d'un document.", "")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Result<GetDocumentInfoResponse>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(Result<Error>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(Result<Error>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(Result<Error>))]
        public Task<Result<GetDocumentInfoResponse>> GetDocumentInfoAsync(
            Guid documentId,
            CancellationToken cancellationToken
        )
        {
            var query = new GetDocumentInfoQuery() { DocumentId = documentId };

            return Mediator.Send(query, cancellationToken);
        }
    }
}
