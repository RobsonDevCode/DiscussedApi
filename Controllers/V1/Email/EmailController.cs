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
        private readonly NLog.ILogger _logger = LogManager.GetCurrentClassLogger();
        public EmailController(IAuthDataAccess authDataAccess, 
            UserManager<User> userManager, IEncryptor encryptor, IEmailProcessing emailProcessing)
        {
            _authDataAccess = authDataAccess;
            _userManager = userManager;
            _encryptor = encryptor;
            _emailProcessing = emailProcessing;
        }

        //[HttpPost("send/recovery")]
        //public async Task<IActionResult> SendRecoveryEmail([FromBody] EmailDto emailRecovery)
        //{
        //    if (string.IsNullOrWhiteSpace(emailRecovery.ToSend)) return BadRequest("Email sent is null");

        //    string body = await _emailSender.GenerateTemplateHtmlBodyAsync(EmailType.Recovery);

        //    if (string.IsNullOrWhiteSpace(body)) return StatusCode(500, "Unable to send recovery email");

        //    await _emailSender.SendAsync(emailRecovery.ToSend, Settings.EmailSettings.RecoverySubject, body);

        //    return Ok();
        //}

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
