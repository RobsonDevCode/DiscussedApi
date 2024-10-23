using DiscussedApi.Models;
using DiscussedApi.Models.Comments;
using DiscussedApi.Processing.Comments;
using Discusseddto.CommentDtos;
using FluentEmail.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NLog;

namespace DiscussedApi.Controllers.V1.Comments
{
    [ApiController]
    [Route("V1/[controller]")]
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
        public async Task<IActionResult> PostCommentAsync([FromBody] NewCommentDto postComment)
        {
            if (string.IsNullOrWhiteSpace(postComment.UserId)) return BadRequest("Login to create a comment");

            if (string.IsNullOrWhiteSpace(postComment.Context)) return BadRequest("Cannot post an empty Comment!");

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
            if (commentToLike.CommentId == null) return BadRequest("Comment id cannot be null");

            if (string.IsNullOrWhiteSpace(commentToLike.UserId)) return BadRequest("User Id liking the comment cannot be null");

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
