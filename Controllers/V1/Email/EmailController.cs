using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using DiscussedApi.Models;
using DiscussedApi.Services.Email;
using DiscussedDto.Email;
using static DiscussedApi.Models.EmailTypeToGenertate;
using NLog;
using DiscussedApi.Reopisitory.Auth;
using Microsoft.AspNetCore.Identity;
using DiscussedApi.Models.UserInfo;
using DiscussedApi.Extentions;
using DiscussedApi.Configuration;
using FluentEmail.Core;
using DiscussedApi.Processing;
using DiscussedApi.Services.Tokens;
using DiscussedApi.Models.ApiResponses;

namespace DiscussedApi.Controllers.V1.Email
{
    [ApiController]
    [Route("V1/[controller]")]
    public class EmailController : ControllerBase
    {
        private readonly IEmailProcessing _emailProcessing;
        private readonly IAuthDataAccess _authDataAccess;
        private readonly UserManager<User> _userManager;
        private readonly IEncryptor _encryptor;
        private readonly ITokenService _tokenService;
        private readonly NLog.ILogger _logger = LogManager.GetCurrentClassLogger();
        public EmailController(IAuthDataAccess authDataAccess,UserManager<User> userManager, 
            IEncryptor encryptor, IEmailProcessing emailProcessing, ITokenService tokenService)
        {
            _authDataAccess = authDataAccess;
            _userManager = userManager;
            _encryptor = encryptor;
            _emailProcessing = emailProcessing;
            _tokenService = tokenService;
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        [HttpPost("send/recovery")]
        public async Task<IActionResult> SendRecoveryEmail([FromBody] EmailDto emailRecovery)
        {
            string email = await _encryptor.DecryptStringAsync(emailRecovery.ToSend, emailRecovery.KeyId);

            var user = await _userManager.FindByEmailAsync(email.ToLower());

            if (user == null)
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = new { email_sent = false }
                });

           string? token = await _tokenService.GenerateAndStorePasswordResetTokenAsync(user, Response);

            if (string.IsNullOrWhiteSpace(token))
                throw new InvalidOperationException("error when generating password reset token, token is null");
            //fire and forget email
             _emailProcessing.SendRecoveryEmail(email, token);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = new { email_sent = true }
            });
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost("send/confirmation")]
        public async Task<IActionResult> SendConfirmationEmail([FromBody] EmailDto confirmation)
        {
            if (string.IsNullOrWhiteSpace(confirmation.ToSend))
                return BadRequest("Email sent is null");


            string email = await _encryptor.DecryptStringAsync(confirmation.ToSend, confirmation.KeyId);

            await _emailProcessing.SendConfirmationEmail(email);

            return Ok();
        }

        //TODO add a topic notification probably better todo in a seperate app 
    }
}
