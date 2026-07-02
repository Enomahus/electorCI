namespace Pcea.Core.Net.Authorization.Application.Attributes
{
    public interface IPermissionsHolder
    {
        public IEnumerable<string> Permissions { get; }
    }
}
