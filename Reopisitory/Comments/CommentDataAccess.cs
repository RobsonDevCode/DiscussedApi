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

        // ******** GET Commands ******** 
        public async Task<List<Comment>> GetCommentsPostedByFollowing(Guid? userId, string topic, long nextPageToken, CancellationToken ctx)
        {
            if (userId == null)
                throw new ArgumentNullException("User id cannot be null when getting comments");

            try
            {
                using (var commentDb = new CommentsDBContext())
                {
                    var result =  await commentDb.Comments
                                                 .Where(c => c.UserId == userId
                                                        && c.DtCreated >= Settings.TimeToGetCommentFrom
                                                        && c.TopicId == topic
                                                        && c.Refernce > nextPageToken)
                                                 .Take(Settings.CommentMax)
                                                 .ToListAsync(ctx);

                    if (result == null)
                        throw new Exception("Null return when retriving commments");

                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                throw;
            }
        }
        public async Task<ImmutableList<Comment>> GetTopCommentsForTodaysTopic(string topic, CancellationToken ctx)
        {
            try
            {
                using (var commentDb = new CommentsDBContext())
                {
                    var comments = await commentDb.Comments
                                                  .Where(x => x.TopicId == topic)
                                                  .OrderByDescending(x => x.Interactions) //sum of likes reposts and replys
                                                  .ThenByDescending(x => x.DtCreated)
                                                  .Take(Settings.CommentMax)
                                                  .ToListAsync(ctx);

                    if (comments == null)
                        throw new Exception("Null return when retriving commments");

                    return comments.ToImmutableList();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                throw;
            }

        }
        public async Task<ImmutableList<Comment>> GetTopCommentsForTodaysTopic(string topic, long? nextPageToken, CancellationToken ctx)
        {
            try
            {
                using (var connection = new CommentsDBContext())
                {
                    var nextPageOfComments = await connection.Comments
                                                             .Where(x => x.TopicId == topic
                                                             && x.Refernce > (nextPageToken == null ? 0 : nextPageToken))
                                                             .OrderByDescending(x => x.Interactions)
                                                             .ThenByDescending(x => x.DtCreated)
                                                             .Take(Settings.CommentMax)
                                                             .ToListAsync(ctx);

                    if (nextPageOfComments == null)
                        throw new Exception("Null return when retriving commments");

                    return nextPageOfComments.ToImmutableList();
                }
            }
            catch(Exception ex)
            {
                _logger.Error(ex, ex.Message);
                throw;
            }
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

                return (result > 0) ? true : false;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                throw;
            }
        }
        public async Task<bool> IsCommentValid(Guid? commentId, CancellationToken ctx)
        {
            if (commentId == null)
                throw new ArgumentNullException("Comment id null when check if valid");

            try
            {
                int result = 0;

                using (var commentsDb = new CommentsDBContext())
                {
                    result = await commentsDb.Comments.CountAsync(x => x.Id.Equals(commentId), ctx);
                }

                return (result > 0) ? true : false;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                throw;
            }
        }


        //******** Post Commands ********
        public async Task PostCommentAsync(Comment comment, CancellationToken ctx)
        {

            try
            {
                using (var commentsDb = new CommentsDBContext())
                {
                    var add = await commentsDb.Comments.AddAsync(comment, ctx);

                    if (await commentsDb.SaveChangesAsync(ctx) == 0)
                        throw new Exception("Query Excecuted but no rows were affected");
                }

            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                throw;
            }
        }

        //******** Update Commands ********
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



                    if (comment.Like) request.Likes++;
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

        public async Task<Comment> UpdateCommentContentAsync(UpdateCommentDto comment, CancellationToken ctx)
        {
            try
            {
                if (string.IsNullOrEmpty(comment.Content))
                    throw new ArgumentNullException("New content is null when attempting to edit content");

                //check if user trying to delete the request is the user who posted it
                if (!await validateUserPostedComment(comment.UserId, comment.CommentId, ctx))
                    throw new Exception("User trying to edit comment did not post it!");

                using (CommentsDBContext commentsDBContext = new())
                {
                    var request = await commentsDBContext.Comments
                                        .FirstOrDefaultAsync(x => x.Id.Equals(comment.CommentId) && x.UserId.Equals(comment.UserId));

                    if (request == null)
                        throw new Exception($"Error while attempting to update content on comment {comment.CommentId}");

                    request.Content = comment.Content;

                    var result = await commentsDBContext.SaveChangesAsync();

                    if (result == 0)
                        throw new Exception("Query was succesfully sent to database but no change in result");

                    return request;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                throw;
            }
            throw new NotImplementedException();
        }

        //******** Delete Commands ********
        public async Task DeleteCommentAsyncEndpoint(Guid commentId, CancellationToken ctx)
        {
            try
            {
                using (CommentsDBContext commentsDB = new())
                {

                    var result = await commentsDB.Comments
                                            .Where(x => x.Id.Equals(commentId))
                                            .ExecuteDeleteAsync(ctx);

                    //Already in trouble at this point, but atleast we flag and it's now got our attention
                    if (result > 0)
                        throw new Exception("query resulted in mutiple rows deleted");
                    else if (result == 0)
                        throw new Exception("query executed but no rows affected");

                }


            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                throw;
            }
        }

        //******** private Commands ********
        private async Task<bool> validateUserPostedComment(Guid userId, Guid commentId, CancellationToken ctx)
        {
            try
            {
                using (CommentsDBContext commentsDBContext = new())
                {
                    return (await commentsDBContext.Comments
                                                    .Where(x => x.Id.Equals(userId) && x.UserId.Equals(userId))
                                                    .CountAsync(ctx) == 1) ? true : false;

                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                throw;
            }
        }

       
    }
}
