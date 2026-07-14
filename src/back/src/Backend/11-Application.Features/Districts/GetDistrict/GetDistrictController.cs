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

namespace Application.Features.Districts.GetDistrict
{
    [ExcludeFromCodeCoverage]
    [ApiController]
    [Route("districts")]
    [OpenApiTag("districts")]
    public class GetDistrictController : ApiControllerBase
    {
        /// <summary>
        /// Get ditrict by Id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [OpenApiOperation("GetDistrict", "Récupère une circonscription par son identifiant.", "")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Result<GetDistrictResponse>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(Result<Error>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(Result<Error>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(Result<Error>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(Result<Error>))]
        public Task<Result<GetDistrictResponse>> GetDistrictAsync(
            long id,
            CancellationToken cancellationToken
        )
        {
            var query = new GetDistrictQuery(id);
            return Mediator.Send(query, cancellationToken);
        }
    }
}
