using DiscussedApi.Common.Validations;
using DiscussedApi.Extentions;
using DiscussedApi.Models.ApiResponses;
using DiscussedApi.Reopisitory.Auth;
using DiscussedApi.Services.Tokens;
using Discusseddto.Auth;
using Discusseddto.User;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DiscussedApi.Controllers.V1.Auth
{
    [ApiController]
    [Route("v1/[controller]")]
    public class TokenController : ControllerBase
    {
        private readonly ITokenService _tokenService;
        private readonly IAuthDataAccess _authDataAccess;
        private readonly IEncryptor _encryptor;
        public TokenController(ITokenService tokenService, IAuthDataAccess authDataAccess, IEncryptor encryptor)
        {
            _tokenService = tokenService;
            _authDataAccess = authDataAccess;
            _encryptor = encryptor;
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("refresh/{username}")]
        public async Task<IActionResult> Refresh(string username)
        {
            var refreshToken = Request.Cookies[$"refresh_token_{username}"];

            if (string.IsNullOrEmpty(refreshToken))
                return BadRequest("No Refresh token given");

            var token = await _tokenService.ProcessRefreshToken(refreshToken, Response);

            if (string.IsNullOrWhiteSpace(token.Jwt) || string.IsNullOrWhiteSpace(token.NewRefreshToken))
            {
                await _tokenService.CleanUpTokens(username, refreshToken, Response);
                return Unauthorized("token no longer valid");
            }

            return Ok(new
            {
                jwt = token.Jwt,
                refresh_token = token.NewRefreshToken
            });
        }

        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost("storekey")]
        public async Task<IActionResult> StoreEncryptionCredentials(EncyrptionCredentialsDto encyrptionCredentials,
            IValidator<EncyrptionCredentialsDto> validator)
        {
            var failedValidation = await Validator<EncyrptionCredentialsDto>.TryValidateRequest(encyrptionCredentials, validator);
            if (failedValidation != null)
                return ValidationProblem(failedValidation);

            await _authDataAccess.StoreKeyAndIvAsync(encyrptionCredentials);

            return Created();
        }

        /// <summary>
        /// PasswordTokenValidation: Validates if the user can access the reset password page
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost("password-validation")]
        public async Task<IActionResult> PasswordTokenValidation([FromBody] string token)
        {
            string unecryptedToken = _tokenService.DecodeUrlSafeToken(token);

            var user = await _authDataAccess.ValidPasswordRefreshToken(unecryptedToken);

            if (!user.IsValid)
                throw new BadHttpRequestException("no token provided");
            
            return Ok(new ApiResponse<object> { 
                Success = true,
                Data = new { email = user.ValidUserEmail}
            });
        }

    }
}
