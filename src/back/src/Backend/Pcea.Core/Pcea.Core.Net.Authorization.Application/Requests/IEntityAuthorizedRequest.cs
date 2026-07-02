using System;
using System.Collections.Generic;
using System.Text;

namespace Pcea.Core.Net.Authorization.Application.Requests
{
    public interface IEntityAuthorizedRequest<out T_EntityId>
    {
        public T_EntityId AuthorizationEntityId { get; }
    }
}
