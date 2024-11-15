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
using System.Collections.Immutable;

namespace DiscussedApi.Reopisitory.Comments
{
    public class CommentDataAccess : ICommentDataAccess
    {
        private readonly NLog.ILogger _logger = LogManager.GetCurrentClassLogger();
        public CommentDataAccess()
        {

        }
       
        public async Task<List<Comment>> GetCommentsForNewUserAsync(Guid? userId, string topic)
        {
            if(userId == null)
                throw new ArgumentNullException($"{nameof(userId)} cannot be null");

            try
            {
                List<Comment> getTop50Liked = new List<Comment>();
                using (var commentDb = new CommentsDBContext())
                {
                     getTop50Liked = await commentDb.Comments
                                               .Where(c => c.TopicId.ToLower() == topic.ToLower())
                                               .OrderByDescending(x => x.DtCreated)
                                               .ThenByDescending(x => x.Likes)
                                               .Take(50)
                                               .ToListAsync();

                    if (getTop50Liked.Count() == 0) throw new Exception("No Comments return when retriving commments for new user");

                }

                return getTop50Liked;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                throw;
            }
        }
        public async Task<ImmutableList<Comment>> GetTopCommentsForTodaysTopic(string topic)
        {
            try
            {
                using (var commentDb = new CommentsDBContext())
                {
                     var comments = await commentDb.Comments
                                                   .Where(s => s.TopicId == topic)
                                                   .OrderByDescending (x => x.DtCreated)
                                                   .ThenByDescending (x => x.Likes)
                                                   .Take(100)
                                                   .ToListAsync();

                    if (comments.Count() == 0) throw new Exception("No Comments return when retriving commments");    
                    
                    return comments.ToImmutableList();
                }
            }
            catch(Exception ex)
            {
                _logger.Error(ex, ex.Message);
                throw;
            }

        }

        public async Task PostCommentAsync(Comment comment, CancellationToken cancellationToken)
        {

            try
            {
                using (var commentsDb = new CommentsDBContext())
                {
                    var add = await commentsDb.Comments.AddAsync(comment, cancellationToken);

                    if (await commentsDb.SaveChangesAsync(cancellationToken) == 0)
                        throw new Exception("Query Excecuted but no rows were affected");
                }

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
                int result = 0;

                using (var commentsDb = new CommentsDBContext())
                {
                     result = await commentsDb.Comments.CountAsync(x => x.Id.Equals(commentId));

                }
                 if (result > 0) return true;

                return false;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                throw;
            }
        }

        public async Task<Comment> UpdateCommentLikesAsync(LikeCommentDto comment)
        {
           
            if (comment == null) 
                throw new ArgumentNullException("Error when updating like count, Comment is null");

            try
            {
                Comment? request = new();

                using (var commentDb = new CommentsDBContext())
                {
                     request = await commentDb.Comments
                                        .FirstOrDefaultAsync(c => c.Id.Equals(comment.CommentId));

                    if (request == null)
                        throw new Exception($"Error while attempting to update likes on comment {comment.CommentId}");

                    

                    if(comment.Like) request.Likes++;
                    else request.Likes--;

                    var result = await commentDb.SaveChangesAsync();

                    if (result == 0) throw new Exception("Query was succesfully sent to database but no change in result");

                }
                return request;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                throw;
            }
        }


        public async Task<List<Comment>> GetCommentsPostedByFollowing(Guid? userId, string topic, CancellationToken cancellationToken)
        {
            if(userId == null)
                throw new ArgumentNullException("User id cannot be null when getting comments");

            try
            {
                var yesterday = DateTime.UtcNow.Date.AddDays(-1);

                using (var commentDb = new CommentsDBContext())
                {
                    return await commentDb.Comments
                                                 .Where(c => c.UserId == userId
                                                        && c.DtCreated >= yesterday
                                                        && c.TopicId.ToLower() == topic.ToLower())
                                                 .Take(Settings.CommentMax)
                                                 .ToListAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                 throw;
            }
        }

        public async Task DeleteCommentAsyncEndpoint(Guid commentId, CancellationToken cancellationToken)
        {
            try
            {
                using (CommentsDBContext commentsDB = new())
                {

                   var result = await commentsDB.Comments
                                           .Where(x => x.Id.Equals(commentId))
                                           .ExecuteDeleteAsync(cancellationToken);

                    //Already in trouble at this point, but atleast we flag and it's now got our attention
                    if (result > 0)
                        throw new Exception("query resulted in mutiple rows deleted");
                    else if (result == 0)
                        throw new Exception("query executed but no rows affected");

                }


            }
            catch(Exception ex) 
            {
                _logger.Error(ex, ex.Message);
                throw;
            }
            throw new NotImplementedException();
        }

       
    }
}
