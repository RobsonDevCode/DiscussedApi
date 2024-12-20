using DiscussedApi.Models;
using static DiscussedApi.Models.EmailTypeToGenertate;

namespace DiscussedApi.Services.Email
{
    public interface IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string body);
        public Task<string> GenerateTemplateHtmlBodyAsync(EmailType emailType);
    }
}
