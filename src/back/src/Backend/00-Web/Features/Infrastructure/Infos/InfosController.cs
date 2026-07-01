using System.Diagnostics.CodeAnalysis;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;

namespace Web.Features.Infrastructure.Infos
{
    [ExcludeFromCodeCoverage]
    [Route("info")]
    [OpenApiTag("info")]
    [Authorize]
    public class InfosController(TimeProvider timeProvider, Version version) : ControllerBase
    {
        private readonly Version _version = version;
        private readonly TimeProvider _timeProvider = timeProvider;

        /// <summary>
        /// Get the date on the server
        /// </summary>
        /// <returns></returns>
        [HttpGet("date")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(DateTime), (int)HttpStatusCode.OK)]
        public ActionResult GetDate()
        {
            return new OkObjectResult(_timeProvider.GetUtcNow());
        }

        /// <summary>
        /// get the timezone on the server
        /// </summary>
        /// <returns></returns>
        [HttpGet("timezone")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        public ActionResult GetTimezone()
        {
            return new OkObjectResult(TimeZoneInfo.Local.StandardName);
        }

        /// <summary>
        /// Get the application version
        /// </summary>
        /// <returns></returns>
        [HttpGet("version")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        public ActionResult GetVersion()
        {
            return new OkObjectResult(_version.ToString());
        }
    }
}
