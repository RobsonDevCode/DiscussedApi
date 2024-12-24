namespace DiscussedApi.Extentions
{
    public interface IEncryptor
    {
        Task<string> DecryptStringAsync(string encyptedPassword, Guid id);
        Task<(string UsernameOrEmail, string Password)> DecryptCredentials(string encryptedEmail, string encryptedPassword, Guid keyId);
    }
}