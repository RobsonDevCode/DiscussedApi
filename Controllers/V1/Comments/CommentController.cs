using DiscussedApi.Common.Validations;
using DiscussedApi.Models;
using DiscussedApi.Models.Comments;
using DiscussedApi.Models.UserInfo;
using DiscussedApi.Processing.Comments;
using DiscussedApi.Validations;
using Discusseddto.CommentDtos;
using FluentEmail.Core;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using NLog;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace DiscussedApi.Controllers.V1.Comments
{
    [Route("V1/[controller]")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private NLog.ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly ICommentProcessing _commentProcessing;

        public CommentController(ICommentProcessing commentProcessing)
        {
             _commentProcessing = commentProcessing;
        }


    

        [Authorize]
        [HttpPost("PostComment")]
        public async Task<IActionResult> PostCommentAsync(NewCommentDto postComment,
            [FromServices] IValidator<NewCommentDto> validator)
        {
            //validate request
            var validate = await Validator<NewCommentDto>.ValidationAsync(postComment, validator);

            if(validate.FaliedValidation != null) return ValidationProblem(validate.FaliedValidation);

            if (string.IsNullOrWhiteSpace(postComment.UserId)) return BadRequest(ModelState);

            try
            {
                await _commentProcessing.PostCommentAsync(postComment);

                return Ok();
            }
            catch(Exception ex)
            {
                _logger.Error(ex, ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        [Authorize]
        [HttpPatch("LikeComment")]
        public async Task<IActionResult> LikeCommentAsync([FromBody] LikeCommentDto commentToLike)
        {
            if (string.IsNullOrWhiteSpace(commentToLike.UserId)) return BadRequest(ModelState);

            try
            {
                var comment = await _commentProcessing.LikeCommentAsync(commentToLike);

                if (comment == null) return StatusCode(500, "Error Comment updated returned null");

                return Ok(comment);
            }

            catch(Exception ex)
            {
                _logger.Error (ex, ex.Message);
                return StatusCode(500, ex.Message);
            }

        }

        [Authorize]
        [HttpGet("GetTodaysComments")]
        public async Task<IActionResult> GetComments([Required(ErrorMessage = "User Id is Required")]string userId)
        {
            try
            {
                if(string.IsNullOrWhiteSpace(userId)) return BadRequest(ModelState);    

                var result = await _commentProcessing.GetCommentsAsync(userId);

                if (result.Count() == 0) return BadRequest("User id given returned no comment content");

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                return StatusCode(500, ex.Message);
            }


        }

        [Authorize]
        [HttpPatch("UnLikeComment")]
        public async Task<IActionResult> UnlikeCommentAsync([FromBody] LikeCommentDto commentToUnlike)
        {
            throw new NotImplementedException();
        }

        [Authorize]
        [HttpPost("PostReplyToComment")]
        public async Task<IActionResult> PostReplyToCommentAsync()
        {
            return Ok();
        }

    }
}
