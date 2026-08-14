namespace Pcea.Core.Net.AuditTrail.Interfaces
{
    public interface IAuditableResult
    {
        public string? Subject { get; }
        public string? AdditionalInfo { get; }
    }
}
