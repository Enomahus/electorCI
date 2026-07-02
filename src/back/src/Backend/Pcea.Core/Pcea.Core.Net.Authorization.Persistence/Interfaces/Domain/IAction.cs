namespace Pcea.Core.Net.Authorization.Persistence.Interfaces.Domain
{
    public interface IAction
    {
        public ICollection<IPermission> ActionPermissions { get; }
    }
}
