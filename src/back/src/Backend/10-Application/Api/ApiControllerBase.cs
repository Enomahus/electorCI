using Microsoft.AspNetCore.Authorization;

namespace Application.Api;

[Authorize]
public class ApiControllerBase : AbstractApiControllerBase { }
