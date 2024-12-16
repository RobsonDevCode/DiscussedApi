namespace DiscussedApi.Extentions
{
    public interface IEncryptor
    {
        Task<string> DecryptPassword(string encyptedPassword, Guid id);

    }
}