using PFMSApi.Models;

namespace PFMSApi.Services.Tokens
{
    public interface ITokenService
    {
        string GeneratedToken(User user);
    }
}
