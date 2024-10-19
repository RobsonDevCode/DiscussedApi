
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using Org.BouncyCastle.Asn1.Tsp;
using PFMSApi.Configuration;
using PFMSApi.Models;
using static PFMSApi.Models.EmailTypeToGenertate;

namespace PFMSApi.Services.Email
{
    public class EmailSender : IEmailSender
    {
        public async Task SendEmailAsync(string email, string subject, string body)
        {
            var emailToSend = new MimeMessage();


            emailToSend.From.Add(MailboxAddress.Parse(Settings.EmailSettings.Mailer));
            emailToSend.To.Add(MailboxAddress.Parse(email));
            emailToSend.Subject = subject;
            emailToSend.Body = new TextPart(TextFormat.Html) { Text = body };


            try
            {
                using (var smtp = new SmtpClient())
                {
                    smtp.Connect("smtp.mailersend.net", 587, SecureSocketOptions.StartTls);
                    smtp.Authenticate(Settings.EmailSettings.Mailer, Settings.EmailSettings.Password);
                    await smtp.SendAsync(emailToSend);
                }

            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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
                    throw new NotImplementedException();
            }

        }
    }
}
