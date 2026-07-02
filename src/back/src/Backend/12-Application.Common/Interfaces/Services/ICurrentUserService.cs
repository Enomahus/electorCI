namespace Application.Common.Interfaces.Services
{
    public interface ICurrentUserService
    {
        Guid? UserId { get; }
        string? UserEmail { get; }
        string? ClientIp { get; }
        string? LanguageCode { get; }

        Task<string?> GetTokenAsync(CancellationToken token = default);
    }
}
