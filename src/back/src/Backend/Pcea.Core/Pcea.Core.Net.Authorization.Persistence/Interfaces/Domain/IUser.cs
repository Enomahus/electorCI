using System;
using System.Collections.Generic;
using System.Text;

namespace Pcea.Core.Net.Authorization.Persistence.Interfaces.Domain
{
    public interface IUser
    {
        public ICollection<IRole> UserRoles { get; }
        public ICollection<IUserEntity> UserUserEntities { get; }
    }
}
