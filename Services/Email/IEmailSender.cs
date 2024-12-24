using DiscussedApi.Models;
using static DiscussedApi.Models.EmailTypeToGenertate;

namespace DiscussedApi.Services.Email
{
    public interface IEmailSender
    {
        Task SendAsync(string toEmail, string subject, string? htmlBody, string? textContent = null);
        Task<string> GenerateTemplateHtmlBodyAsync(EmailType emailType);
    }
}
