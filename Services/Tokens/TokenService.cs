using Microsoft.IdentityModel.Tokens;
using PFMSApi.Configuration;
using PFMSApi.Models;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
namespace PFMSApi.Services.Tokens
{
    public class TokenService : ITokenService
    {
        private readonly SymmetricSecurityKey _key;
        public TokenService() 
        {
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Settings.JwtSettings.Key));
        }
        public string GeneratedToken(User user)
        {
            try
            {
                if (string.IsNullOrEmpty(user.UserName) || string.IsNullOrEmpty(user.Email))
                {
                    throw new ArgumentNullException("Username or Email cannot be null");
                }

                var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.GivenName, user.UserName)
                };

                var credentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddDays(1),
                    SigningCredentials = credentials,
                    Issuer = Settings.JwtSettings.Issuer,
                    Audience = Settings.JwtSettings.Audience
                };

                var tokenHandler = new JwtSecurityTokenHandler();

                var token = tokenHandler.CreateToken(tokenDescriptor);

                return tokenHandler.WriteToken(token);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);

            }
            throw new NotImplementedException();
        }
    }
}
