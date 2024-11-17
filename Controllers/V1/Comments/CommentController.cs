using DiscussedApi.Common.Validations;
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
using Newtonsoft.Json;
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
        private readonly UserManager<User> _userManager;
        private readonly ICommentProcessing _commentProcessing;

        public CommentController(ICommentProcessing commentProcessing, UserManager<User> userManager)
        {
             _commentProcessing = commentProcessing;
            _userManager = userManager;
        }


        // ********** GET COMMANDS **********
        /// <summary>
        /// GetComments: Get Comments based on user prefernce 
        /// </summary>
        /// <param name="userId">User we're getting comments for</param>
        /// <param name="ctx">Handle cancel requests</param>
        /// <returns>List of comments unqiue to the user</returns>
        [Authorize]
        [HttpGet("GetTodaysComments")]
        public async Task<IActionResult> GetComments(Guid userId, string topicName, CancellationToken ctx)
        {

            try
            {
                var result = await _commentProcessing.GetCommentsAsync(userId, topicName, ctx);

                if (result.Count() == 0)
                    throw new Exception("User id given returned no comment content");

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                return StatusCode(500, ex.Message);
            }


        }

        /// <summary>
        /// GetCommentsWihoutSignIn: Load Comments for user's who havent signed in
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="topic"></param>
        /// <returns>Status code and a list of comments </returns>
        [HttpGet("GetCommentsNoSignIn")]
        public async Task<IActionResult> GetCommentsWihoutSignIn(CancellationToken ctx, string topic)
        {
            try
            {
                return Ok(await _commentProcessing.GetCommentsWithNoSignInAsync(ctx, topic));
            }
            catch (Exception ex)
            {
                _logger.Fatal(ex, ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        // ********** POST COMMANDS **********

        /// <summary>
        /// PostCommentAsync: Http post handler used to post comments to the database 
        /// </summary>
        /// <param name="postComment">Comment To Post</param>
        /// <param name="ctx">Ctx used to cancel request</param>
        /// <param name="validator">Validation on the new comment to see if the comment fits requirements</param>
        /// <returns>Status Code</returns>
        [Authorize]
        [HttpPost("PostComment")]
        public async Task<IActionResult> PostCommentAsync(NewCommentDto postComment,CancellationToken ctx, 
            [FromServices] IValidator<NewCommentDto> validator)
        {
            //validate request
            var validate = await Validator<NewCommentDto>.ValidationAsync(postComment, validator);

            if(validate.FaliedValidation != null) return ValidationProblem(validate.FaliedValidation);

            try
            {
               //validate is user calling is an active account, this slows system down but its an extra saftey net 
               if(await _userManager.FindByIdAsync(postComment.UserId.ToString()) == null)
               {
                    _logger.Error("Error trying to post comment with User id that doesnt exist");
                    return BadRequest("Error trying to post comment with User id that doesnt exist");
               }

                await _commentProcessing.PostCommentAsync(postComment, ctx);

                return Ok();
            }
            catch(Exception ex)
            {
                _logger.Error(ex, ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        // ********** PUT And PATCH COMMANDS **********

        /// <summary>
        /// EditLikesOnCommentAsync: HttpPatch used to update the like count on a comment, either a like or unlike
        /// </summary>
        /// <param name="commentToEdit">Comment targeted to like or unlike</param>
        /// <param name="validator">Validation on the request</param>
        /// <returns>Updated Comment</returns>
        [Authorize]
        [HttpPatch("EditLikesOnCommentAsync")]
        public async Task<IActionResult> EditLikesOnCommentAsync([FromBody] LikeCommentDto commentToEdit, 
                                                                 [FromServices] IValidator<LikeCommentDto> validator)
        {
            //validate request
            var validate = await Validator<LikeCommentDto>.ValidationAsync(commentToEdit, validator);

            if (validate.FaliedValidation != null) return ValidationProblem(validate.FaliedValidation);

            try
            {
                //validate is user calling is an active account, this slows system down but its an extra saftey net 
                if (await _userManager.FindByIdAsync(commentToEdit.UserId.ToString()) == null)
                {
                    _logger.Error("Error trying to post comment with User id that doesnt exist");
                    return BadRequest("Error trying to post comment with User id that doesnt exist");
                }

                var comment = await _commentProcessing.LikeOrDislikeCommentAsync(commentToEdit);

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
        [HttpPatch("EditCommentContext")]
        public async Task<IActionResult> EditCommentContextAsync([FromBody] UpdateCommentDto updateComment, 
                                                                 IValidator<UpdateCommentDto> validator, 
                                                                 CancellationToken ctx)
        {
            try
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
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                return StatusCode(500, ex.Message);
            }
        }


        // ********** DELETE COMMANDS **********

        /// <summary>
        /// DeleteComment: Deletes a comment based on comment Id
        /// </summary>
        /// <param name="commentId">Comment id used for deletion</param>
        /// <param name="ctx">Handles canceled requests</param>
        /// <returns>Status code</returns>
        [Authorize]
        [HttpDelete("DeleteComment")]
        public async Task<IActionResult> DeleteComment([Required(ErrorMessage = "Comment Id is required when attempting to delete")] Guid commentId, CancellationToken ctx)
        {
            try
            {
                await _commentProcessing.DeleteCommentAsync(commentId, ctx);

                return Ok();
            } 
            catch(Exception ex) 
            {
                _logger.Error(ex, ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

       


    }
}
