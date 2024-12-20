namespace DiscussedApi.Extentions
{
    public interface IEncryptor
    {
        Task<string> DecryptString(string encyptedPassword, Guid id);

        Task<(string Email, string Password)> DecryptCredentials(string encryptedEmail, string encryptedPassword, Guid keyId);
    }
}