using DiscussedApi.Models;
using DiscussedApi.Models.Comments;
using DiscussedApi.Reopisitory;
using Discusseddto;
using Discusseddto.Comment;
using Discusseddto.CommentDtos;
using NLog;
using System.Collections.Generic;

namespace DiscussedApi.Processing.Comments
{
    public class CommentProcessing : ICommentProcessing
    {
        private static ICommentDataAccess _commentDataAcces;
        private readonly NLog.ILogger _logger = LogManager.GetCurrentClassLogger();
        public CommentProcessing(ICommentDataAccess commentDataAccess)
        {
             _commentDataAcces = commentDataAccess;
        }
        public Task<List<Comment>> GetCommentsAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<Comment> LikeCommentAsync(LikeCommentDto likeCommentDto)
        {
            try
            {
                if (likeCommentDto.CommentId == null) throw new ArgumentException("CommentId is null");

                if (!await isCommentValid(likeCommentDto.CommentId)) throw new KeyNotFoundException("Comment not found or has been deleted");

                return await _commentDataAcces.UpdateCommentLikesAsync(likeCommentDto);
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
                    UserId = newComment.UserId,
                    Context = newComment.Content,
                    ReplyCount = 0,
                    Likes = 0,
                    DtCreated = DateTime.UtcNow,
                    DtUpdated = DateTime.UtcNow,
                };

                 await _commentDataAcces.PostCommentAsync(comment);
            }
            catch (Exception ex) 
            {
                _logger.Error(ex, ex.Message);
                throw;
            }

        }

        private async Task<bool> isCommentValid(Guid? commentId) => 
            await _commentDataAcces.IsCommentValid(commentId);
        
    }
}
