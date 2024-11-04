﻿using DiscussedApi.Data.UserComments;
using DiscussedApi.Models.Comments;
using DiscussedApi.Models.UserInfo;
using Discusseddto;
using Discusseddto.Comment;
using Discusseddto.CommentDtos;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace DiscussedApi.Reopisitory.Comments
{
    public class CommentDataAccess : ICommentDataAccess
    {
        CommentsDBContext _commentDbAccess = new();
        private readonly NLog.ILogger _logger = LogManager.GetCurrentClassLogger();
        public CommentDataAccess()
        {

        }

        public Task DeleteCommentAsyncEndpoint(User user)
        {
            throw new NotImplementedException();
        }

        public Task<Dictionary<List<Comment>, List<Reply>>> GetTopLikedCommentsAsyncEndPoint()
        {

            throw new NotImplementedException();
        }
        public async Task<List<Comment>> GetCommentsForNewUserAsync(string userId)
        {
            try
            {
                var getTop50Liked = await _commentDbAccess.Comments
                                           .OrderByDescending(x => x.Likes)
                                           .Take(50)
                                           .ToListAsync();

                if (getTop50Liked.Count() == 0) throw new Exception("No Comments return when retriving commments for new user");

                return getTop50Liked;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                throw;
            }
            throw new NotImplementedException();
        }

        public async Task PostCommentAsync(Comment comment)
        {

            try
            {
                var add = await _commentDbAccess.Comments.AddAsync(comment);
                var result = await _commentDbAccess.SaveChangesAsync();

                if (result == 0) throw new Exception("Query Excecuted but no rows were affected");

            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                throw;
            }
        }

        public Task<Dictionary<Comment, List<Reply>>> UpdateComment(User user, Comment comment)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> IsCommentValid(Guid? commentId)
        {
            try
            {
                if (commentId == null)
                {
                    throw new ArgumentNullException("Comment id null when check if valid");
                }

                var result = await _commentDbAccess.Comments.CountAsync(x => x.Id.Equals(commentId));

                if (result > 0) return true;

                return false;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                throw;
            }
        }

        public async Task<Comment> UpdateCommentLikesAsync(LikeCommentDto likedComment)
        {
            try
            {
                if (likedComment == null) throw new ArgumentNullException("Error when updating like count, Comment is null");

                var request = await _commentDbAccess.Comments.FirstOrDefaultAsync(x => x.Id == likedComment.CommentId);

                if (request == null)
                {
                    throw new Exception($"Error while attempting to update likes on comment {likedComment.CommentId}");
                }

                request.Likes++;

                var result = await _commentDbAccess.SaveChangesAsync();

                if (result == 0) throw new Exception("Query was succesfully sent to database but no change in result");

                return request;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                throw;
            }
        }

      
    }
}
