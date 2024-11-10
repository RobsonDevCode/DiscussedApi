using DiscussedApi.Configuration;
using DiscussedApi.Data.UserComments;
using DiscussedApi.Models.Comments;
using DiscussedApi.Models.Profiles;
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

        public async Task<List<Comment>> GetTopLikedCommentsAsyncEndPoint(List<Guid?> userIds)
        {
            try
            {

            }
            catch(Exception ex)
            {

            }
            throw new NotImplementedException();
        }
        public async Task<List<Comment>> GetCommentsForNewUserAsync(string userId)
        {
            if(string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException($"{nameof(userId)} cannot be null");

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

        public async Task PostCommentAsync(Comment comment, CancellationToken cancellationToken)
        {

            try
            {
                var add = await _commentDbAccess.Comments.AddAsync(comment, cancellationToken);
                var result = await _commentDbAccess.SaveChangesAsync(cancellationToken);

                if (result == 0) 
                    throw new Exception("Query Excecuted but no rows were affected");

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
            if (commentId == null)
                throw new ArgumentNullException("Comment id null when check if valid");

            try
            {
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
           
            if (likedComment == null) 
                throw new ArgumentNullException("Error when updating like count, Comment is null");

            try
            {
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

        public async Task<List<Comment>> GetCommentsPostedByFollowing(List<Guid?> userIds)
        {
            if(!userIds.Any())
                throw new ArgumentNullException("user ids cannot be null ");

            try
            {
                var yesterday = DateTime.UtcNow.Date.AddDays(-1);

                List<Comment> comments= new();

                foreach (var userId in userIds)
                {
                    comments.AddRange(await _commentDbAccess.Comments
                                                             .Where(c => c.UserId == userId.ToString() && c.DtCreated >= yesterday)//TODO: optimise make string a guid
                                                             .OrderByDescending(x => x.Likes)                           
                                                             .ThenByDescending(x => x.DtCreated)
                                                             .Take(Settings.CommentMax)
                                                             .ToListAsync());

                }

                return comments;
            }

            catch(Exception ex)
            {
                throw;
            }
        }

        public async Task<List<Comment>> GetCommentsPostedByFollowing(Guid? userId, CancellationToken cancellationToken)
        {
            if(userId == null)
                throw new ArgumentNullException("User id cannot be null when getting comments");

            try
            {
                var yesterday = DateTime.UtcNow.Date.AddDays(-1);

                return await _commentDbAccess.Comments
                                             .Where(c => c.UserId == userId.ToString() && c.DtCreated >= yesterday)//TODO: optimise make string a guid
                                             .Take(Settings.CommentMax)
                                             .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                 throw;
            }
        }

    }
}
