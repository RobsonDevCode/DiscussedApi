using DiscussedApi.Common.Validations;
using DiscussedApi.Extentions;
using DiscussedApi.Models.Comments;
using DiscussedApi.Models.UserInfo;
using DiscussedApi.Processing.Comments;
using DiscussedApi.Processing.Replies;
using DiscussedApi.Reopisitory.Comments;
using Discusseddto.CommentDtos.ReplyDtos;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using NLog;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;

namespace DiscussedApi.Controllers.V1.Reply
{
    [Route("v1/[controller]")]
    [ApiController]
    public class ReplyController : ControllerBase
    {
        private readonly NLog.ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly IReplyProcessing _replyProcessing;
        private readonly ICommentDataAccess _commentDataAccess;
        private readonly IMemoryCache _memoryCache;
        private readonly UserManager<User> _userManager;

        public ReplyController(IReplyProcessing replyProcessing, ICommentDataAccess commentDataAccess,
            IMemoryCache memoryCache, UserManager<User> userManager)
        {
            _replyProcessing = replyProcessing;
            _commentDataAccess = commentDataAccess;
            _memoryCache = memoryCache;
            _userManager = userManager;
        }


        [Authorize]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost("post")]
        public async Task<IActionResult> PostResultAsync([FromBody] PostReplyDto postReplyDto, [FromServices] IValidator<PostReplyDto> validator,
            CancellationToken ctx)
        {
            var valid = await Validator<PostReplyDto>.ValidationAsync(postReplyDto, validator);

            if (valid.FaliedValidation != null) return ValidationProblem(valid.FaliedValidation);

            if (!await _commentDataAccess.IsCommentValid(postReplyDto.CommentId, ctx))
                return BadRequest("error while replying to comment, comment cannot be found or has been deleted!");

            await _replyProcessing.PostReplyAsync(postReplyDto, ctx);

            return Created();
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("get/{commentId}")]
        public async Task<IActionResult> GetRepliesAsync([Required] Guid commentId, [FromQuery(Name = "nextpage_token")] string? encyptedNextPageToken, CancellationToken ctx)
        {
            if (!await _commentDataAccess.IsCommentValid(commentId, ctx))
                return BadRequest($"error while getting replies for comment {commentId}, comment cannot be found or has been deleted!");

            using var timeOutCts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ctx, timeOutCts.Token);

            string key = $"{commentId}-reply-cache";
            long? nextPageToken = null;

            if (!string.IsNullOrWhiteSpace(encyptedNextPageToken))
            {
                var settings = CachingSettings.GenerateCacheKeyFromToken(encyptedNextPageToken, key);
                key = settings.Key;
                nextPageToken = settings.Token;
            }

            var result = await _memoryCache.GetOrCreateAsync(key, async entry =>
            {
                entry.SetOptions(CachingSettings.SetReplyCommentSettings());
                return await _replyProcessing.GetReplysForCommentAsync(commentId, linkedCts.Token);
            });

            if (result == null)
            {
                _logger.Warn($"No Reply Content for comment {commentId}");
                return NoContent();
            }

            return Ok(result);
        }

        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPatch("edit/likes")]
        public async Task<IActionResult> UpdateReplyLikes([FromBody] EditReplyLikesDto editReplyLikesDto,
            CancellationToken ctx,
            [FromServices] IValidator<EditReplyLikesDto> validator)
        {
            var validate = await Validator<EditReplyLikesDto>.ValidationAsync(editReplyLikesDto, validator);

            if (validate.FaliedValidation != null)
                return ValidationProblem(validate.FaliedValidation);

            if (await _userManager.FindByIdAsync(editReplyLikesDto.UserId.ToString()) == null)
            {
                _logger.Error("Error trying to like comment with User id that doesnt exist");
                return BadRequest("Error trying to like comment with User id that doesnt exist");
            }

            return Ok(await _replyProcessing.EditReplyLikesAsync(editReplyLikesDto, ctx));
        }

        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpDelete("delete/{replyId}/{commentId}/{userId}")]
        public async Task<IActionResult> DeleteReply(Guid replyId, Guid commentId ,
                string userId, CancellationToken ctx)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return ValidationProblem("user_id cannot be null when deleting a reply");


            return Ok(await _replyProcessing.DeleteReplyAsync(replyId, commentId, userId, ctx));
        }

    }
}
