using DiscussedApi.Common.Validations;
using DiscussedApi.Models.Comments;
using DiscussedApi.Processing.Comments;
using DiscussedApi.Processing.Replies;
using DiscussedApi.Reopisitory.Comments;
using Discusseddto.CommentDtos.ReplyDtos;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using NLog;

namespace DiscussedApi.Controllers.V1.Reply
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReplyController : ControllerBase
    {
        private readonly NLog.ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly IReplyProcessing _replyProcessing;
        private readonly ICommentDataAccess _commentDataAccess;
        public ReplyController(IReplyProcessing replyProcessing, ICommentDataAccess commentDataAccess)
        {
            _replyProcessing = replyProcessing;
            _commentDataAccess = commentDataAccess;
        }


        [Authorize]
        [HttpPost("post")]
        public async Task<IActionResult> PostResultAsync([FromBody] PostReplyDto postReplyDto,[FromServices] IValidator<PostReplyDto> validator,
            CancellationToken ctx)
        {
            try
            {
                var valid= await Validator<PostReplyDto>.ValidationAsync(postReplyDto, validator);

                if (valid.FaliedValidation != null) return ValidationProblem(valid.FaliedValidation);

                if (!await _commentDataAccess.IsCommentValid(postReplyDto.CommentId, ctx))
                    return BadRequest("error while replying to comment, comment cannot be found or has been deleted!");

                await _replyProcessing.PostReplyAsync(postReplyDto, ctx);

                return Ok();
            }
            catch (Exception ex) 
            {
                _logger.Error(ex, ex.Message);
                return StatusCode(500, ex.Message);
            }

        }

        [HttpGet("{commentId}/replies")]
        public async Task<IActionResult> GetRepliesAsync(Guid commentId, CancellationToken ctx)
        {
            try
            {
                if(!await _commentDataAccess.IsCommentValid(commentId, ctx))
                    return BadRequest($"error while getting replies for comment {commentId}, comment cannot be found or has been deleted!");

                //TODO add memeory caching 
                //TODO add paging


                var result = await _replyProcessing.GetReplysForCommentAsync(commentId, ctx);

                if (result.Replies.Count == 0)
                {
                    _logger.Warn($"No Reply Content for comment {commentId}");
                    return NoContent();
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                return StatusCode(500, ex.Message);
            }
        }
        

    }
}
