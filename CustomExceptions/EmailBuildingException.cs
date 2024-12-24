using DiscussedApi.Models;
using static DiscussedApi.Models.EmailTypeToGenertate;

namespace DiscussedApi.CustomExceptions
{
    public class EmailBuildingException : Exception
    {
        public EmailBuildingException()
            : base("An Exception occured while building an email")
        {

        }
        public EmailBuildingException(string message) : base(message) { }
        public EmailBuildingException(EmailType emailType) : base(getErrorMessage(emailType))
        {
        }

        private static string? getErrorMessage(EmailType emailType)
        {
            return emailType switch
            {
                EmailType.Confirmation => "An Exception Occured while building the confirmation email",
                EmailType.Recovery => "An Exception Occured while building the recovery email",
                _ => "An Unexpected error occured while building an email"
            };
        }


    }
}
