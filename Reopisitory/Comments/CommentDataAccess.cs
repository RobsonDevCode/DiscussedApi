using DiscussedApi.Abstraction;
using DiscussedApi.Configuration;
using DiscussedApi.Data.UserComments;
using DiscussedApi.Models.Comments;
using DiscussedApi.Models.Profiles;
using DiscussedApi.Models.UserInfo;
using DiscussedApi.Reopisitory.DataMapping;
using Discusseddto;
using Discusseddto.Comment;
using Discusseddto.CommentDtos;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using NLog;
using System.Collections.Immutable;

namespace DiscussedApi.Reopisitory.Comments
{
    public class CommentDataAccess : ICommentDataAccess
    {
        private readonly NLog.ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly IMySqlConnectionFactory _mySqlConnectionFactory;
        private readonly CommentsDBContext _commentContext;
        public CommentDataAccess(CommentsDBContext context, IMySqlConnectionFactory mySqlConnectionFactory)
        {
            _commentContext = context;
            _mySqlConnectionFactory = mySqlConnectionFactory;
        }

        // ******** GET Commands ******** 
        public async Task<List<Comment>> GetCommentsPostedByFollowing(Guid? userId, string topic, long nextPageToken, CancellationToken ctx)
        {
            if (userId == null)
                throw new ArgumentNullException("User id cannot be null when getting comments");

            string sql = @"SELECT Id, UserId, Content, ReplyCount, 
                                      Likes, DtCreated, DtUpdated,
                                      TopicId, Interactions, UserName, Refernce
                                      FROM comments 
                                      WHERE UserId = @userId 
                                      AND DtCreated >= @dtFrom 
                                      AND TopicId = @topicId
                                      AND Refernce > @nextPageToken LIMIT @commentMax 
                                      ORDER BY interactions DESC, DtUpdated desc;"
                                       ;


            await using MySqlConnection connection = _mySqlConnectionFactory.CreateUserInfoConnection();
            await connection.OpenAsync(ctx);

            await using MySqlCommand cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@dtFrom", Settings.TimeToGetCommentFrom);
            cmd.Parameters.AddWithValue("@topicId", topic);
            cmd.Parameters.AddWithValue("@nextPageToken", nextPageToken);
            cmd.Parameters.AddWithValue("@commentMac", Settings.CommentMax);

            await using MySqlDataReader reader = await cmd.ExecuteReaderAsync(ctx);

            List<Comment> comments = new List<Comment>();

            while (await reader.ReadAsync(ctx))
            {
                comments.Add(RepositoryMappers.MapComment(reader));
            }

            if (comments == null)
                throw new Exception("Null return when retriving commments");

            return comments;
        }


        public async Task<ImmutableList<Comment>> GetTopCommentsForTodaysTopic(string topic, long? nextPageToken, CancellationToken ctx)
        {
            string sql = @"SELECT Id, UserId, Content, ReplyCount, 
                                  Likes, DtCreated, DtUpdated,
                                  TopicId, Interactions, UserName, Refernce
                           FROM comments
                           WHERE TopicId = @topicId AND 
                           Refernce > @nextPageToken
                           ORDER BY interactions DESC, DtUpdated desc;";

            await using MySqlConnection connection = _mySqlConnectionFactory.CreateUserInfoConnection();

            await connection.OpenAsync(ctx);
            await using MySqlCommand cmd = new(sql, connection);
            cmd.Parameters.AddWithValue("@topicId", topic);
            cmd.Parameters.AddWithValue("@nextPageToken", (nextPageToken == null) ? 0 : nextPageToken);

            await using MySqlDataReader reader = await cmd.ExecuteReaderAsync(ctx);

            List<Comment> comments = new List<Comment>();

            while (await reader.ReadAsync())
            {
                var comment = RepositoryMappers.MapComment(reader);
                comments.Add(comment);
            }

            if (comments.Count == 0)
                throw new Exception("Null return when retriving commments");

            return comments.ToImmutableList();
        }

        public async Task<bool> IsCommentValid(Guid? commentId)
        {
            if (commentId == null)
                throw new ArgumentNullException("Comment id null when check if valid");

            await using MySqlConnection connection = _mySqlConnectionFactory.CreateUserInfoConnection();

            await connection.OpenAsync();

            await using MySqlCommand cmd =
                new(@"SELECT COUNT(*)  FROM comments WHERE id = @id;", connection);
            cmd.Parameters.AddWithValue("@id", commentId);

            object? result = await cmd.ExecuteScalarAsync();

            if (result == null)
                return false;

            return (Convert.ToInt16(result) > 0) ? true : false;
        }

        public async Task<bool> IsCommentValid(Guid? commentId, CancellationToken ctx)
        {
            if (commentId == null)
                throw new ArgumentNullException("Comment id null when check if valid");

            await using MySqlConnection connection = _mySqlConnectionFactory.CreateUserInfoConnection();

            await connection.OpenAsync(ctx);

            await using MySqlCommand cmd =
                new(@"SELECT COUNT(*)  FROM comments WHERE id = @id;", connection);
            cmd.Parameters.AddWithValue("@id", commentId);

            object? result = await cmd.ExecuteScalarAsync(ctx);

            if (result == null)
                return false;

            return (Convert.ToInt16(result) > 0) ? true : false;
        }


        //******** Post Commands ********
        public async Task PostCommentAsync(NewCommentDto comment, CancellationToken ctx)
        {
            string sql = @"INSERT INTO comments (Id, UserId, Content, ReplyCount, 
                                                 Likes, DtCreated, DtUpdated,
                                                 TopicId, Interactions, UserName)
            			   VALUES(@id, @userId, @content, 0, 
			              	      0, NOW(), NOW(), @topicId, 
                                  0, @userName);";

            await using MySqlConnection connection = _mySqlConnectionFactory.CreateUserInfoConnection();
            await connection.OpenAsync(ctx);

            await using MySqlCommand cmd = new(sql, connection);

            cmd.Parameters.AddWithValue("@id", comment.Id);
            cmd.Parameters.AddWithValue("@userId", comment.UserId);
            cmd.Parameters.AddWithValue("@content", comment.Content);
            cmd.Parameters.AddWithValue("@topicId", comment.TopicId);
            cmd.Parameters.AddWithValue("@userName", comment.UserName);

            if (await cmd.ExecuteNonQueryAsync(ctx) == 0)
                throw new Exception("Query Excecuted but no rows were affected");
        }

        //******** Update Commands ********
        public async Task<string> UpdateCommentLikesAsync(LikeCommentDto comment, CancellationToken ctx)
        {
            string sql;
            string statusMessage;
            if (comment.IsLike)
            {
                sql = "UPDATE comments SET likes = likes + 1  WHERE id = @id";
                statusMessage = $"{comment.UserId} Successfully liked reply {comment.CommentId}";
            }
            else
            {
                sql = "UPDATE comments SET likes = likes - 1  WHERE id = @id";
                statusMessage = $"{comment.UserId} Successfully unliked reply {comment.CommentId}";
            }

            await using MySqlConnection connection = _mySqlConnectionFactory.CreateUserInfoConnection();
            await connection.OpenAsync();

            await using MySqlCommand cmd = new(sql, connection);

            cmd.Parameters.AddWithValue("@id", comment.CommentId);

            await cmd.ExecuteNonQueryAsync(ctx);

            return statusMessage;
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

                var request = await _commentContext.Comments
                                    .FirstOrDefaultAsync(x => x.Id.Equals(comment.CommentId) && x.UserId.Equals(comment.UserId));

                if (request == null)
                    throw new Exception($"Error while attempting to update content on comment {comment.CommentId}");

                request.Content = comment.Content;

                var result = await _commentContext.SaveChangesAsync();

                if (result == 0)
                    throw new Exception("Query was succesfully sent to database but no change in result");

                return request;
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

                var result = await _commentContext.Comments
                                        .Where(x => x.Id.Equals(commentId))
                                        .ExecuteDeleteAsync(ctx);

                //Already in trouble at this point, but atleast we flag and it's now got our attention
                if (result > 0)
                    throw new Exception("query resulted in mutiple rows deleted");
                else if (result == 0)
                    throw new Exception("query executed but no rows affected");

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
                return (await _commentContext.Comments
                                                .Where(x => x.Id.Equals(userId) && x.UserId.Equals(userId))
                                                .CountAsync(ctx) == 1) ? true : false;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                throw;
            }
        }


    }
}
