using DiscussedApi.Models.Auth;
using Discusseddto.Auth;

namespace DiscussedApi.Reopisitory.Auth
{
    public interface IAuthDataAccess
    {
        Task DeleteTokenByIdAsync(string refreshToken);
        Task<RefreshToken?> GetRefreshTokenByIdAsync(string tokenSent);
        Task<ResetPasswordToken?> GetPasswordTokenAsync(string tokenSent);
        Task StoreRefreshTokenAsync(RefreshToken token);
        Task StoreKeyAndIvAsync(EncyrptionCredentialsDto encyrptionCredentials);
        Task StoreEmailConfirmationCodeAsync(string email, int confirmationCode);
        Task<bool> IsConfirmationCodeCorrect(int confirmationCode);
        Task<(bool IsValid, string? ValidUserEmail)> ValidPasswordRefreshToken(string token);
        Task<EncyrptionCredentialsDto?> GetKeyAndIvAsync(Guid id);
        Task DeleteKeyAndIvByIdAsync(Guid id);
        Task StorePassordResetToken(string? email, string token);
    }
}
