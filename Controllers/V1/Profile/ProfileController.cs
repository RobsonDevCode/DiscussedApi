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
        [HttpPost("follow")]
        public async Task<IActionResult> FollowUser(ProfileDto followUser, [FromServices] IValidator<ProfileDto> validator, CancellationToken ctx)
        {
            var failedValidation = await Validator<ProfileDto>.TryValidateRequest(followUser, validator);

            if (failedValidation != null) 
                return ValidationProblem(failedValidation);

            await _profileProcessing.FollowUser(followUser, ctx);

            return Ok();
        }

        [Authorize]
        [HttpPost("unfollow")]
        public async Task<IActionResult> UnfollowUser(ProfileDto unfollow, [FromServices] IValidator<ProfileDto> validator, CancellationToken ctx)
        {
            var failedValidation = await Validator<ProfileDto>.TryValidateRequest(unfollow, validator);

            if (failedValidation != null) 
                return ValidationProblem(failedValidation);

            await _profileProcessing.UnfollowUser(unfollow, ctx);

            return Ok();

        }
    }
}
