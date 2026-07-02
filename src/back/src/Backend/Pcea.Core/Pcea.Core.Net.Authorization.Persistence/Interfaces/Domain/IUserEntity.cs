namespace Pcea.Core.Net.Authorization.Persistence.Interfaces.Domain
{
    public interface IUserEntity
    {
        public ICollection<IRole> UserEntityRoles { get; }
        public IUser? UserEntityUser { get; }
        public IEntity? UserEntityEntity { get; }
    }
}
