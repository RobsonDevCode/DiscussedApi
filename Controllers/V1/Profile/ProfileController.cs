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

        [Authorize]
        [HttpPost("FollowUser")]
        public async Task<IActionResult> FollowUser(ProfileDto followUser, [FromServices] IValidator<ProfileDto> validator)
        {
            var validateRequest = await Validator<ProfileDto>.ValidationAsync(followUser, validator);

            if (validateRequest.FaliedValidation != null) return ValidationProblem(validateRequest.FaliedValidation);

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

        [Authorize]
        [HttpPost("UnfollowUser")]
        public async Task<IActionResult> UnfollowUser(ProfileDto unfollow, [FromServices] IValidator<ProfileDto> validator)
        {
            var validateRequest = await Validator<ProfileDto>.ValidationAsync(unfollow, validator);

            if(validateRequest.FaliedValidation != null) return ValidationProblem(validateRequest.FaliedValidation);

            try
            {
                await _profileProcessing.UnfollowUser(unfollow);

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
