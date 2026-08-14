using System.Diagnostics.CodeAnalysis;
using Application.Api;
using Application.Common.Enums;
using Application.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Tools.Exceptions.Errors;

namespace Application.Features.Citizens.SearchCitizen
{
    [ExcludeFromCodeCoverage]
    [ApiController]
    [Route("citizens")]
    [OpenApiTag("citizens")]
    public class SearchCitizenController : ApiControllerBase
    {
        /// <summary>
        /// Get a citizen
        /// </summary>
        /// <param name="searchTerm"></param>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("search")]
        [OpenApiOperation(
            "SearchCitizen",
            "Recherche un citoyen par son nom, son prénom, non lieu de naissance.",
            ""
        )]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Result<List<SearchCitizenResponse>>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(Result<Error>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(Result<Error>))]
        public Task<Result<List<SearchCitizenResponse>>> SearchCitizenAsync(
            [FromQuery] string? searchTerm,
            [FromQuery] Guid? id,
            [FromQuery] Gender gender,
            CancellationToken cancellationToken
        )
        {
            var query = new SearchCitizenQuery()
            {
                SearchTerm = searchTerm,
                Id = id,
                Gender = gender,
            };
            return Mediator.Send(query, cancellationToken);
        }
    }
}
