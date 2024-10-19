using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PFMSApi.Models;
using PFMSApi.Services.Email;
using PFMSDdto.Email;
using static PFMSApi.Models.EmailTypeToGenertate;

namespace PFMSApi.Controllers.V1.Email
{
    [ApiController]
    [Route("V1/[controller]")]
    public class EmailController : ControllerBase
    {
        private readonly IEmailSender _emailSender;
        public EmailController(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        [HttpPost("SendRecoveryEmail")]
        public async Task<IActionResult> SendRecoveryEmail([FromBody] EmailDto emailRecovery)
        {
            try
            {
                string body = await _emailSender.GenerateHtmlBodyAsync(EmailType.Recovery);

                if (string.IsNullOrWhiteSpace(body)) return StatusCode(500, "Unable to send recovery email");

                await _emailSender.SendEmailAsync(emailRecovery.Email, emailRecovery.Subject, body);

                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(500, ex.Message);
            }
        }

        [Authorize]
        [HttpPost("ConfirmEmail")]
        public async Task<IActionResult> SendConfirmationEmail([FromBody]EmailDto confirmation)
        {
            if (string.IsNullOrWhiteSpace(confirmation.Email)) return BadRequest("Email sent is null");

            string body = await _emailSender.GenerateHtmlBodyAsync(EmailType.Confirmation);

            if (string.IsNullOrWhiteSpace(body)) return StatusCode(500, "Unable to send Confirmation Email");

            await _emailSender.SendEmailAsync(confirmation.Email, confirmation.Subject, body);

            return Ok();
        }
    }
}
