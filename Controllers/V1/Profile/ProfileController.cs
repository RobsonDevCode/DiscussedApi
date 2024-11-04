using DiscussedApi.Common.Validations;
using DiscussedApi.Processing.Profile;
using Discusseddto.Profile;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NLog;

namespace DiscussedApi.Controllers.V1.Profile
{
    [Route("V1/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private NLog.ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly IProfileProcessing _profileProcessing; 
        public ProfileController(IProfileProcessing profileProcessing)
        {
             _profileProcessing = profileProcessing;
        }

        [HttpPost("FollowUser")]
        public async Task<IActionResult> FollowUser(ProfileDto followUser, [FromServices] IValidator<ProfileDto> validator)
        {
           var validateRequest = await Validator<ProfileDto>.ValidationAsync(followUser, validator);

           if (validateRequest.FaliedValidation != null) return ValidationProblem(validateRequest.FaliedValidation);

           if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                await _profileProcessing.FollowUser(followUser);

                return Ok();
            }

            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
               return StatusCode(500, ex.Message);
            }
        }
    }
}
