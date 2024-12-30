namespace DiscussedApi.Models.Auth
{
    public class ResetPasswordToken
    {
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresOnUtc { get; set; }
    }
}
