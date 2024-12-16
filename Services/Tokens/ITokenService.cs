using DiscussedApi.Models.Auth;
using DiscussedApi.Models.UserInfo;

namespace DiscussedApi.Services.Tokens
{
    public interface ITokenService
    {
        string GeneratedToken(User user);
        Task<(string Jwt, RefreshToken RefreshToken)> GenerateAndSetJwtAndRefreshToken(User user, HttpResponse response);
        Task<(string? Jwt, string? NewRefreshToken)> ProcessRefreshToken(string tokenSent, HttpResponse response);
        Task CleanUpTokens(string username, string refreshToken, HttpResponse response);
    }
}
