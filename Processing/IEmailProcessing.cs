namespace DiscussedApi.Processing
{
    public interface IEmailProcessing
    {
        Task SendConfirmationEmail(string email);
        Task SendRecoveryEmail(string email);
    }
}