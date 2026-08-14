using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Application.Api;
using Application.Exceptions;
using Application.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Tools.Exceptions.Errors;

namespace Application.Features.Document.DocumentDownload
{
    [ExcludeFromCodeCoverage]
    [ApiController]
    [Route("documents")]
    [OpenApiTag("documents")]
    public class DocumentDownloadController : ApiControllerBase
    {
        /// <summary>
        /// Download a file
        /// </summary>
        /// <param name="documentId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("{documentId}/download")]
        [OpenApiOperation("DownloadDocument", "Télécharge un document.", "")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileResult))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(Result<Error>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(Result<Error>))]
        public async Task<IActionResult> DownloadDocumentAsync(
            Guid documentId,
            CancellationToken cancellationToken
        )
        {
            var query = new DocumentDownloadQuery() { DocumentId = documentId };

            var response =
                (await Mediator.Send(query, cancellationToken)).Data
                ?? throw new NotFoundException("Document", documentId);

            return File(response.Content, response.ContentType, response.FileName);
        }
    }
}
