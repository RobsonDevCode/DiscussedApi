
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using Org.BouncyCastle.Asn1.Tsp;
using DiscussedApi.Configuration;
using DiscussedApi.Models;
using static DiscussedApi.Models.EmailTypeToGenertate;

namespace DiscussedApi.Services.Email
{
    public class EmailSender : IEmailSender
    {
        private readonly NLog.ILogger _logger = NLog.LogManager.GetCurrentClassLogger();
        public async Task SendEmailAsync(string email, string subject, string body)
        {
            var emailToSend = new MimeMessage();

            try
            {
                emailToSend.From.Add(MailboxAddress.Parse(Settings.EmailSettings.Mailer));
                emailToSend.To.Add(MailboxAddress.Parse(email));
                emailToSend.Subject = subject;
                emailToSend.Body = new TextPart(TextFormat.Html) { Text = body };

                using (var smtp = new SmtpClient())
                {
                    smtp.Connect("smtp.mailersend.net", 587, SecureSocketOptions.StartTls);
                    smtp.Authenticate(Settings.EmailSettings.Mailer, Settings.EmailSettings.Password);
                    await smtp.SendAsync(emailToSend);
                }

            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                await Task.FromException(ex);
            }
        }

        public async Task<string> GenerateHtmlBodyAsync(EmailType emailType)
        {

            switch(emailType)
            {
                case EmailType.Recovery:
                    return await File.ReadAllTextAsync(Settings.EmailSettings.RecoveryHtmlBodyFilePath);

                case EmailType.Confirmation:
                    return await File.ReadAllTextAsync(Settings.EmailSettings.ConfirmationBodyFilePath);

                default:
                    throw new NotImplementedException("Email Type given is invalid or isn't implimented on the version being used");
            }

        }
    }
}
