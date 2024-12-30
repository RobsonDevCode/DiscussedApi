using DiscussedApi.Models.Auth;
using DiscussedApi.Models.UserInfo;

namespace DiscussedApi.Services.Tokens
{
    public interface ITokenService
    {
        string GeneratedToken(User user);
        Task GenerateAndSetJwtAndRefreshToken(User user, HttpResponse response);
        Task<string?> GenerateAndStorePasswordResetTokenAsync(User user, HttpResponse response);
        Task<bool> IsValidPasswordResetToken(string token);
        Task<(string? Jwt, string? NewRefreshToken)> ProcessRefreshToken(string tokenSent, HttpResponse response);
        Task CleanUpTokens(string username, string refreshToken, HttpResponse response);
        public string ConvertToUrlSafeToken(string token);
        public string DecodeUrlSafeToken(string urlSafeToken);
    }
}
