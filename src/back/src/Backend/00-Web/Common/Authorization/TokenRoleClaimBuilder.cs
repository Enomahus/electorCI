using Pcea.Core.Net.Authorization.Web.Services;

namespace Web.Common.Authorization
{
    public class TokenRoleClaimBuilder : TokenRoleClaimBuilder<long>
    {
        protected override long ParseEntityId(string entityId)
        {
            return long.Parse(entityId);
        }
    }
}
