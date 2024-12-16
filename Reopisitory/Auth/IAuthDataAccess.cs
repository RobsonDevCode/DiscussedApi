using DiscussedApi.Models.Auth;
using Discusseddto.Auth;

namespace DiscussedApi.Reopisitory.Auth
{
    public interface IAuthDataAccess
    {
        Task DeleteTokenById(string refreshToken);
        Task<RefreshToken?> GetTokenById(string tokenSent);
        Task StoreRefreshToken(RefreshToken token);
        Task StoreKeyAndIv(EncyrptionCredentialsDto encyrptionCredentials);
        Task<EncyrptionCredentialsDto?> GetKeyAndIv(Guid id);
    }
}
