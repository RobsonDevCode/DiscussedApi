﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using DiscussedApi.Models;
using DiscussedApi.Services.Email;
using DiscussedDto.Email;
using static DiscussedApi.Models.EmailTypeToGenertate;
using NLog;

namespace DiscussedApi.Controllers.V1.Email
{
    [ApiController]
    [Route("V1/[controller]")]
    public class EmailController : ControllerBase
    {
        private readonly IEmailSender _emailSender;
        private readonly NLog.ILogger _logger = LogManager.GetCurrentClassLogger();
        public EmailController(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        [HttpPost("send/recovery")]
        public async Task<IActionResult> SendRecoveryEmail([FromBody] EmailDto emailRecovery)
        {
            if (string.IsNullOrWhiteSpace(emailRecovery.Email)) return BadRequest("Email sent is null");

            string body = await _emailSender.GenerateHtmlBodyAsync(EmailType.Recovery);

            if (string.IsNullOrWhiteSpace(body)) return StatusCode(500, "Unable to send recovery email");

            await _emailSender.SendEmailAsync(emailRecovery.Email, emailRecovery.Subject, body);

            return Ok();
        }

        [Authorize]
        [HttpPost("send/confirmation")]
        public async Task<IActionResult> SendConfirmationEmail([FromBody] EmailDto confirmation)
        {
            if (string.IsNullOrWhiteSpace(confirmation.Email)) return BadRequest("Email sent is null");

            string body = await _emailSender.GenerateHtmlBodyAsync(EmailType.Confirmation);

            if (string.IsNullOrWhiteSpace(body)) return StatusCode(500, "Unable to send Confirmation Email");

            await _emailSender.SendEmailAsync(confirmation.Email, confirmation.Subject, body);

            return Ok();
        }

        //TODO add a topic notification probably better todo in a seperate app 
    }
}
