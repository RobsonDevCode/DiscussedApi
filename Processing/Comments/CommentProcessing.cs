using DiscussedApi.Models;
using DiscussedApi.Models.Comments;
using DiscussedApi.Models.UserInfo;
using DiscussedApi.Reopisitory.Comments;
using DiscussedApi.Reopisitory.Profiles;
using Discusseddto;
using Discusseddto.Comment;
using Discusseddto.CommentDtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MimeKit.Cryptography;
using NLog;
using System.Collections.Generic;

namespace DiscussedApi.Processing.Comments
{
    public class CommentProcessing : ICommentProcessing
    {
        private readonly ICommentDataAccess _commentDataAccess;
        private readonly IProfileDataAccess _profileDataAccess;
        private readonly UserManager<User> _userManager;
        private readonly NLog.ILogger _logger = LogManager.GetCurrentClassLogger();
        public CommentProcessing(ICommentDataAccess commentDataAccess, IProfileDataAccess profileDataAccess, UserManager<User> userManager)
        {
             _commentDataAccess = commentDataAccess;
            _profileDataAccess = profileDataAccess;
        }
        public async Task<List<Comment>> GetCommentsAsync(string userId)
        {
            var userFollowing = await _profileDataAccess.GetUserFollowing(userId);

            if (userFollowing.Count() == 0) throw new Exception("User follows no accounts");

            var toplikedComments = await _commentDataAccess.GetTopLikedCommentsAsyncEndPoint();

            //if user following is null check if they are a new user and display content based on prompts selected
            if (userFollowing == null) 
            {
                if (!await isNewUser(userId)) throw new Exception("User follows no accounts");

                return await _commentDataAccess.GetCommentsForNewUserAsync(userId);
            }

            throw new NotImplementedException();
        }

        public async Task<Comment> LikeCommentAsync(LikeCommentDto likeCommentDto)
        {
            try
            {
                if (likeCommentDto.CommentId == null) throw new ArgumentException("CommentId is null");

                if (!await isCommentValid(likeCommentDto.CommentId)) throw new KeyNotFoundException("Comment not found or has been deleted");

                return await _commentDataAccess.UpdateCommentLikesAsync(likeCommentDto);
            }
            catch (Exception ex) 
            {
              _logger.Error(ex, ex.Message);
               throw;
            }
        }

        public async Task PostCommentAsync(NewCommentDto newComment)
        {

            if (newComment == null)
            {
                _logger.Error("Error Posting Comment: Comment is null");
                throw new ArgumentNullException("Comment was null");
            }

            if (string.IsNullOrWhiteSpace(newComment.UserId))
            {
                _logger.Error("Error Posting Comment: User info is null when attempting to post a comment");
                 throw new ArgumentNullException("User was null");
            }

            try
            {
                Comment comment = new()
                {
                    Id = newComment.Id,
                    TopicId = newComment.TopicId,
                    UserId = newComment.UserId,
                    Context = newComment.Content,
                    ReplyCount = 0,
                    Likes = 0,
                    DtCreated = DateTime.UtcNow,
                    DtUpdated = DateTime.UtcNow,
                };

                await _commentDataAccess.PostCommentAsync(comment);
            }
            catch (Exception ex) 
            {
                _logger.Error(ex, ex.Message);
                throw;
            }

        }

        private async Task<bool> isCommentValid(Guid? commentId) => 
            await _commentDataAccess.IsCommentValid(commentId);
        
        private async Task<bool> isNewUser(string userId)
        {
            try
            {
                User? user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == userId &&
                                                                         x.CreatedAt < DateTime.UtcNow.AddDays(-(int)DateTime.UtcNow.DayOfWeek - 3));
                if(user == null) return false;

                return true;
            }
            catch(Exception ex)
            {
                _logger.Error(ex, ex.Message);
                throw;
            }
        }
    }
}
