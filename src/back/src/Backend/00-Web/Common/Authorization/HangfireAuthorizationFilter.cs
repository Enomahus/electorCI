using Hangfire.Dashboard;

namespace Web.Common.Authorization
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();

            return httpContext.User.Identity?.IsAuthenticated
                ?? false && httpContext.User.Identity.Name == "pcea_admin";
        }
    }
}
