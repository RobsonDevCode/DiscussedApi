using DiscussedApi.Configuration;
using DiscussedApi.CustomExceptions;
using DiscussedApi.Extentions;
using DiscussedApi.Models.UserInfo;
using DiscussedApi.Reopisitory.Auth;
using DiscussedApi.Services.Email;
using Microsoft.AspNetCore.Identity;
using static DiscussedApi.Models.EmailTypeToGenertate;

namespace DiscussedApi.Processing
{
    public class EmailProcessing : IEmailProcessing
    {
        private readonly IAuthDataAccess _authDataAccess;
        private readonly IEmailSender _emailSender; 
        private readonly IEncryptor _encryptor;
        private readonly UserManager<User> _userManager;
        public EmailProcessing(IEmailSender emailSender, IAuthDataAccess authDataAccess, IEncryptor encryptor
            ,UserManager<User> userManager)
        {
            _authDataAccess = authDataAccess;
            _emailSender = emailSender;
            _encryptor = encryptor;
            _userManager = userManager;
        }

        public async Task SendConfirmationEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentNullException("email is null");

            string body = await _emailSender.GenerateTemplateHtmlBodyAsync(EmailType.Confirmation);
            if (string.IsNullOrWhiteSpace(body))
                throw new Exception("unable to read confirmation email body html");


            Random rand = new Random();
            int confirmationNumber = rand.Next(100000, 1000000);
            body = body.Replace("{confirmationCode}", confirmationNumber.ToString());

            await _authDataAccess.StoreEmailConfirmationCodeAsync(email, confirmationNumber);
            await _emailSender.SendAsync(email, Settings.EmailSettings.ConfirmationSubject, body);

        }

        public async Task SendRecoveryEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                throw new EmailBuildingException("email is null when sending recovery email");

            string body = await _emailSender.GenerateTemplateHtmlBodyAsync(EmailType.Recovery);

            if (string.IsNullOrWhiteSpace(body))
                throw new EmailBuildingException(EmailType.Recovery);
        }
    }
}
