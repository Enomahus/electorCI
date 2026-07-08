namespace Application.Features.Security.Common
{
    public class TokenResponse(string accessToken, string refreshToken)
    {
        public string AccessToken { get; set; } = accessToken;
        public string RefreshToken { get; set; } = refreshToken;
    }
}
