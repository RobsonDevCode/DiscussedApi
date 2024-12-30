using Microsoft.IdentityModel.Tokens;
using DiscussedApi.Configuration;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using DiscussedApi.Models.UserInfo;
using System.Security.Cryptography;
using DiscussedApi.Models.Auth;
using DiscussedApi.Reopisitory.Auth;
using Microsoft.AspNetCore.Identity;
using DiscussedApi.Authenctication;
using NLog;
namespace DiscussedApi.Services.Tokens
{
    public class TokenService : ITokenService
    {
        private readonly SymmetricSecurityKey _key;
        private readonly IAuthDataAccess _authDataAccess;
        private readonly UserManager<User> _userManager;
        private readonly NLog.ILogger _logger = LogManager.GetCurrentClassLogger();
        public TokenService(IAuthDataAccess authDataAccess, UserManager<User> userManager)
        {
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Settings.JwtSettings.Key));
            _authDataAccess = authDataAccess;
            _userManager = userManager;
        }

        public async Task GenerateAndSetJwtAndRefreshToken(User user, HttpResponse response)
        {
            if (user == null)
                throw new BuildTokenException("Error when building JWT user is null");

            if (string.IsNullOrWhiteSpace(user.UserName))
                throw new BuildTokenException("Error when building JWT username is null");


            var jwt = GeneratedToken(user);
            setJWTInCookies(response, jwt, user.UserName);

            var refreshToken = generateRefreshToken(user.UserName);

            if (string.IsNullOrWhiteSpace(refreshToken.Token))
                throw new BuildTokenException($"Error when building Refresh Token when presented user {user.UserName}");

            setRefreshTokenInCookie(response, refreshToken, user.UserName);

            await _authDataAccess.StoreRefreshTokenAsync(refreshToken);
        }
        public async Task<string?> GenerateAndStorePasswordResetTokenAsync(User user, HttpResponse response)
        {
            if(user == null)
                throw new BuildTokenException("Error building Password Reset Token user is null");

            if (string.IsNullOrWhiteSpace(user.UserName))
                throw new BuildTokenException("Error building Password Reset Token username is null");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            
            setPasswordResetToken(response, token, user.UserName);

            await _authDataAccess.StorePassordResetToken(user.Email, token);

            return ConvertToUrlSafeToken(token);
        }

        public string ConvertToUrlSafeToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new BuildTokenException("Error building url safe token, token is null");

            return token
                    .Replace("+", "-")
                    .Replace("/", "_")
                    .TrimEnd('=');
        } 

        public string DecodeUrlSafeToken(string urlSafeToken)
        {
            string token = urlSafeToken
             .Replace("-", "+")
             .Replace("_", "/");

            // Add padding if necessary
            int padding = 4 - (token.Length % 4);
            if (padding < 4)
            {
                token += new string('=', padding);
            }

            return token;
        }
        public async Task<bool> IsValidPasswordResetToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new Exception("token can't be null");

            var storedToken = await _authDataAccess.GetPasswordTokenAsync(token);

            if (storedToken == null)
            {
                _logger.Warn("pasword token returned null");
                return false;
            }

            if(storedToken.ExpiresOnUtc < DateTime.UtcNow)
                return false;

            return true;
        }
      
        public string GeneratedToken(User user)
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
                Expires = DateTime.UtcNow.AddMinutes(Settings.JwtSettings.JwtExpiresFrom),
                SigningCredentials = credentials,
                Issuer = Settings.JwtSettings.Issuer,
                Audience = Settings.JwtSettings.Audience
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }


        public async Task<(string? Jwt, string? NewRefreshToken)> ProcessRefreshToken(string tokenSent, HttpResponse response)
        {
            var token = await _authDataAccess.GetRefreshTokenByIdAsync(tokenSent);

            if (token == null || token.ExpiresOnUtc < DateTime.UtcNow)
                return (null, null);

            var user = await _userManager.FindByNameAsync(token.Username);
            if (user == null)
                throw new Exception($"User {token.Username} returned null");



            string jwt = GeneratedToken(user);
            setJWTInCookies(response, jwt, token.Username);

            RefreshToken refreshToken = generateRefreshToken(token.Username);

            setRefreshTokenInCookie(response, refreshToken, token.Username);

            return (jwt, refreshToken.Token);
        }

        public async Task CleanUpTokens(string username, string refreshToken, HttpResponse response)
        {
            deleteTokensFromCookies(username, response);
            await _authDataAccess.DeleteTokenByIdAsync(refreshToken);
        }

        private void deleteTokensFromCookies(string username, HttpResponse response)
        {
            response.Cookies.Delete($"access_token_{username}", new CookieOptions()
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            });

            response.Cookies.Delete($"refresh_token_{username}", new CookieOptions()
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            });
        }

        private RefreshToken generateRefreshToken(string userName)
        {
            var randomBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            var token = Convert.ToBase64String(randomBytes);

            return new RefreshToken()
            {
                Username = userName,
                Token = token,
                ExpiresOnUtc = DateTime.UtcNow.AddHours(Settings.JwtSettings.RefreshTokenExpiresFrom)
            };
        }

        private void setJWTInCookies(HttpResponse response, string jwt, string username)
        {
            response.Cookies.Append($"access_token_{username}", jwt, new CookieOptions()
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddMinutes(Settings.JwtSettings.JwtExpiresFrom)
            });
        }
        private void setRefreshTokenInCookie(HttpResponse response, RefreshToken refreshToken, string username)
        {
            response.Cookies.Append($"refresh_token_{username}", refreshToken.Token, new CookieOptions()
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = refreshToken.ExpiresOnUtc,
            });
        }

        private void setPasswordResetToken(HttpResponse response, string token, string username)
        {
            response.Cookies.Append($"reset_password_token_{username}", token, new CookieOptions()
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict, 
                Expires = DateTime.UtcNow.AddMinutes(Settings.Encryption.PasswordResetExpireTime)
            });
        }


    }
}
