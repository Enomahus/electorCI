namespace Pcea.Core.Net.Authorization.Persistence.Interfaces.Domain
{
    public interface IRole
    {
        public ICollection<IAction> RoleActions { get; }
    }
}
