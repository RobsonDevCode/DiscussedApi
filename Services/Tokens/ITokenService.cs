using DiscussedApi.Models.UserInfo;

namespace DiscussedApi.Services.Tokens
{
    public interface ITokenService
    {
        string GeneratedToken(User user);
    }
}
