using PFMSApi.Models;
using static PFMSApi.Models.EmailTypeToGenertate;

namespace PFMSApi.Services.Email
{
    public interface IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string body);
        public Task<string> GenerateHtmlBodyAsync(EmailType emailType);
    }
}
