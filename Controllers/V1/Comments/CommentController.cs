using DiscussedApi.Common.Validations;
using DiscussedApi.Extentions;
using DiscussedApi.Models;
using DiscussedApi.Models.Comments;
using DiscussedApi.Models.UserInfo;
using DiscussedApi.Processing.Comments;
using DiscussedApi.Validations;
using Discusseddto.CommentDtos;
using Discusseddto.CommentDtos.ReplyDtos;
using FluentEmail.Core;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using NLog;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Security.Cryptography;

namespace DiscussedApi.Controllers.V1.Comments
{
    [Route("V1/[controller]")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private NLog.ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly UserManager<User> _userManager;
        private readonly ICommentProcessing _commentProcessing;
        private readonly IMemoryCache _memoryCache;

        public CommentController(ICommentProcessing commentProcessing, UserManager<User> userManager, IMemoryCache memoryCache)
        {
            _commentProcessing = commentProcessing;
            _userManager = userManager;
            _memoryCache = memoryCache;

        }


        // ********** GET COMMANDS **********

        /// <summary>
        ///  GetTopComments: Get Comments based on user prefernce 
        /// </summary>
        /// <param name="topicName">Name of the topic we're getting comments from</param>
        /// <param name="ctx">cancellation token</param>
        /// <param name="encryptedToken">Next page token</param>
        /// <returns cref="IActionResult">Http Status Code</returns>
        /// <exception cref="CryptographicException"></exception>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("{topicName}/comments")]
        public async Task<IActionResult> GetTopComments([Required] string topicName, CancellationToken ctx, string? encryptedToken)
        {
            //set token to null 
            string key = "comment-key";
            long? nextPageToken = null;

            using var timeOutCts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ctx, timeOutCts.Token);

            //check if encypted next page token is present
            if (!string.IsNullOrWhiteSpace(encryptedToken))
            {
                var settings = CachingSettings.GenerateCacheKeyFromToken(encryptedToken, key);
                key = settings.Key;
                nextPageToken = settings.Token;
            }

            //cache comment data 
            var result = await _memoryCache.GetOrCreateAsync(key,
                                async entry =>
                                {
                                    entry.SetOptions(CachingSettings.SetCommentCacheSettings());
                                    return await _commentProcessing.GetTopCommentsAsync(topicName, nextPageToken, linkedCts.Token);
                                });

            if (result == null)
                throw new Exception($"Comments returned empty likely due to a query issue");

            if (result.Count == 0)
                throw new Exception($"Comments returned empty likely due to a query issue");

            return Ok(result);
        }

        /// <summary>
        /// GetFollowingComments: Get comments for the following tab,this handler returnes a list of comments from user following.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="topicName"></param>
        /// <param name="ctx"></param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("{userId}/following/{topicName}/comments")]
        public async Task<IActionResult> GetFollowingComments([Required] Guid userId,
            [Required] string topicName,
            string? encryptedToken, CancellationToken ctx)
        {
            //set token to null 
            string key = $"{userId}-following-key";
            long? nextPageToken = null;

            //check if encypted next page token is present
            if (!string.IsNullOrWhiteSpace(encryptedToken))
            {
                var settings = CachingSettings.GenerateCacheKeyFromToken(encryptedToken, key);
                key = settings.Key;
                nextPageToken = settings.Token;
            }

            var result = await _memoryCache.GetOrCreateAsync(key,
                async entry =>
                {
                    entry.SetOptions(CachingSettings.SetCommentCacheSettings());
                    return await _commentProcessing.GetFollowingCommentsAsync(userId, topicName, nextPageToken, ctx);
                });

            if (result == null)
                throw new Exception($"Null was returned taking {userId} as an argument");

            if (result.Count == 0)
                return NoContent();

            return Ok(result);
        }


        // ********** POST COMMANDS **********

        /// <summary>
        /// PostCommentAsync: Http post handler used to post comments to the database 
        /// </summary>
        /// <param name="postComment">Comment To Post</param>
        /// <param name="ctx">Ctx used to cancel request</param>
        /// <param name="validator">Validation on the new comment to see if the comment fits requirements</param>
        /// <returns>Status Code</returns>
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost("post")]
        public async Task<IActionResult> PostCommentAsync([FromBody] NewCommentDto postComment, CancellationToken ctx,
            [FromServices] IValidator<NewCommentDto> validator)
        {
            //validate request
            var validate = await Validator<NewCommentDto>.ValidationAsync(postComment, validator);

            if (validate.FaliedValidation != null) return ValidationProblem(validate.FaliedValidation);

            //validate is user calling is an active account, this slows system down but its an extra saftey net 
            if (await _userManager.FindByIdAsync(postComment.UserId.ToString()) == null)
            {
                _logger.Error("Error trying to post comment with User id that doesnt exist");
                return BadRequest("Error trying to post comment with User id that doesnt exist");
            }

            await _commentProcessing.PostCommentAsync(postComment, ctx);

            return Created();
        }

        // ********** PUT And PATCH COMMANDS **********

        /// <summary>
        /// EditLikesOnCommentAsync: HttpPatch used to update the like count on a comment, either a like or unlike
        /// </summary>
        /// <param name="commentToEdit">Comment targeted to like or unlike</param>
        /// <param name="validator">Validation on the request</param>
        /// <returns>Updated Comment</returns>
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPatch("edit/likes")]
        public async Task<IActionResult> EditLikesOnCommentAsync([FromBody] LikeCommentDto commentToEdit,
                                                                 [FromServices] IValidator<LikeCommentDto> validator,
                                                                 CancellationToken ctx)
        {
            //validate request
            var validate = await Validator<LikeCommentDto>.ValidationAsync(commentToEdit, validator);

            if (validate.FaliedValidation != null) return ValidationProblem(validate.FaliedValidation);

            //validate is user calling is an active account, this slows system down but its an extra saftey net 
            if (await _userManager.FindByIdAsync(commentToEdit.UserId.ToString()) == null)
            {
                _logger.Error("Error trying to like comment with User id that doesnt exist");
                return BadRequest("Error trying to like comment with User id that doesnt exist");
            }

            var comment = await _commentProcessing.LikeOrDislikeCommentAsync(commentToEdit, ctx);

            if (comment == null) return StatusCode(500, "Error Comment updated returned null");

            return Ok(comment);

        }

        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPatch("edit/content")]
        public async Task<IActionResult> EditCommentContextAsync([FromBody] UpdateCommentDto updateComment,
                                                                 IValidator<UpdateCommentDto> validator,
                                                                 CancellationToken ctx)
        {
            var validate = await Validator<UpdateCommentDto>.ValidationAsync(updateComment, validator);

            if (validate.FaliedValidation != null)
                return ValidationProblem(validate.FaliedValidation);

            Comment updatedComment = await _commentProcessing.EditCommentContentAsync(updateComment, ctx);

            if (updatedComment == null)
            {
                _logger.Info($"Comment returned null when updating {updateComment.CommentId}");
                return NoContent();
            }

            return Ok(updatedComment);
        }


        // ********** DELETE COMMANDS **********

        /// <summary>
        /// DeleteComment: Deletes a comment based on comment Id
        /// </summary>
        /// <param name="commentId">Comment id used for deletion</param>
        /// <param name="ctx">Handles canceled requests</param>
        /// <returns>Status code</returns>
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpDelete("{commentId}/delete")]
        public async Task<IActionResult> DeleteComment([Required(ErrorMessage = "Comment Id is required when attempting to delete")] Guid commentId, CancellationToken ctx)
        {
            await _commentProcessing.DeleteCommentAsync(commentId, ctx);
            return Ok();
        }

    }
}
