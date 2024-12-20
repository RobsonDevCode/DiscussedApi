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
using Newtonsoft.Json.Linq;
using DiscussedApi.Authenctication;
namespace DiscussedApi.Services.Tokens
{
    public class TokenService : ITokenService
    {
        private readonly SymmetricSecurityKey _key;
        private readonly IAuthDataAccess _authDataAccess;
        private readonly UserManager<User> _userManager;
        public TokenService(IAuthDataAccess authDataAccess, UserManager<User> userManager)
        {
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Settings.JwtSettings.Key));
            _authDataAccess = authDataAccess;
            _userManager = userManager;
        }

        public async Task<(string Jwt, RefreshToken RefreshToken)> GenerateAndSetJwtAndRefreshToken(User user, HttpResponse response)
        {
            if (user == null)
                throw new BuildTokenException($"Error when building JWT user is null");

            if (string.IsNullOrWhiteSpace(user.UserName))
                throw new BuildTokenException($"Error when building JWT username is null");


            var jwt = GeneratedToken(user);
            setJWTInCookies(response, jwt, user.UserName);

            var refreshToken = generateRefreshToken(user.UserName);

            if(string.IsNullOrWhiteSpace(refreshToken.Token))
                throw new BuildTokenException($"Error when building Refresh Token when presented user {user.UserName}");

            setRefreshTokenInCookie(response, refreshToken, user.UserName);

            await _authDataAccess.StoreRefreshTokenAsync(refreshToken);

            return (jwt, refreshToken);
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
            var token = await _authDataAccess.GetTokenByIdAsync(tokenSent);

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

        public void deleteTokensFromCookies(string username, HttpResponse response)
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

        public RefreshToken generateRefreshToken(string userName)
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

        public void setJWTInCookies(HttpResponse response, string jwt, string username)
        {
            response.Cookies.Append($"access_token_{username}", jwt, new CookieOptions()
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddMinutes(Settings.JwtSettings.JwtExpiresFrom)
            });
        }
        public void setRefreshTokenInCookie(HttpResponse response, RefreshToken refreshToken, string username)
        {
            response.Cookies.Append($"refresh_token_{username}", refreshToken.Token, new CookieOptions()
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = refreshToken.ExpiresOnUtc,
            });
        }



    }
}
