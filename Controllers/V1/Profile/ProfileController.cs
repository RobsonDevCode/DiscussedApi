﻿using DiscussedApi.Common.Validations;
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


        [HttpPost("follow")]
        public async Task<IActionResult> FollowUser(ProfileDto followUser, [FromServices] IValidator<ProfileDto> validator)
        {
            var validateRequest = await Validator<ProfileDto>.ValidationAsync(followUser, validator);

            if (validateRequest.FaliedValidation != null) return ValidationProblem(validateRequest.FaliedValidation);

            await _profileProcessing.FollowUser(followUser);

            return Ok();
        }

        [Authorize]
        [HttpPost("unfollow")]
        public async Task<IActionResult> UnfollowUser(ProfileDto unfollow, [FromServices] IValidator<ProfileDto> validator)
        {
            var validateRequest = await Validator<ProfileDto>.ValidationAsync(unfollow, validator);

            if (validateRequest.FaliedValidation != null) return ValidationProblem(validateRequest.FaliedValidation);

            await _profileProcessing.UnfollowUser(unfollow);

            return Ok();

        }
    }
}
