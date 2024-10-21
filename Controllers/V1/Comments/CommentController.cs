using DiscussedApi.Models;
using DiscussedApi.Models.Comments;
using DiscussedApi.Processing.Comments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NLog;

namespace DiscussedApi.Controllers.V1.TimeManagement
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
        public async Task<IActionResult> PostComment([FromBody]User user, Comment comment)
        {
            if (string.IsNullOrWhiteSpace(user.Id)) return BadRequest("Login to create a comment");

            if (string.IsNullOrWhiteSpace(comment.Context)) return BadRequest("Cannot post an empty Comment!");

            try
            {
                var result = await _commentProcessing.PostCommentAsync(user, comment);

                if(!result.Succeeded)
                {
                    _logger.Warn("Processing Error: " ,result.Errors);
                    return BadRequest(result.Errors);
                }
                return Ok();
            }
            catch(Exception ex)
            {
                _logger.Error(ex, ex.Message);
                return StatusCode(500, ex);
            }
            throw new NotImplementedException();
        }

    }
}
