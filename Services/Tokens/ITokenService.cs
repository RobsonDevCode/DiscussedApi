using DiscussedApi.Models;

namespace DiscussedApi.Services.Tokens
{
    public interface ITokenService
    {
        string GeneratedToken(User user);
    }
}
