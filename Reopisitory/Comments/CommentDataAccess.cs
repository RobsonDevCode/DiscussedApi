using DiscussedApi.Abstraction;
using DiscussedApi.Configuration;
using DiscussedApi.Models.Comments;
using DiscussedApi.Models.Profiles;
using DiscussedApi.Models.UserInfo;
using DiscussedApi.Reopisitory.DataMapping;
using Discusseddto;
using Discusseddto.Comment;
using Discusseddto.CommentDtos;
using MailKit;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using NLog;
using System.Collections.Immutable;
using System.Threading;

namespace DiscussedApi.Reopisitory.Comments
{
    public class CommentDataAccess : ICommentDataAccess
    {
        private readonly IMySqlConnectionFactory _mySqlConnectionFactory;
        private readonly IRepositoryMapper _repositoryMapper;
        public CommentDataAccess(IMySqlConnectionFactory mySqlConnectionFactory,
            IRepositoryMapper repositoryMapper)
        {
            _mySqlConnectionFactory = mySqlConnectionFactory;
            _repositoryMapper = repositoryMapper;
        }

        // ******** GET Commands ******** 
        public async Task<Comment?> GetComment(Guid commentId, CancellationToken ctx)
        {
            await using MySqlConnection connection = _mySqlConnectionFactory.CreateUserInfoConnection();
            await connection.OpenAsync(ctx);

            await using MySqlCommand cmd = new(@"SELECT Id, UserId, Content, ReplyCount, 
                                      Likes, DtCreated, DtUpdated,
                                      TopicId, Interactions, UserName, Reference
                                      FROM comments WHERE id = @id", connection);

            cmd.Parameters.Add("@id", MySqlDbType.Guid).Value = commentId;

            await using MySqlDataReader reader = await cmd.ExecuteReaderAsync(ctx);
            while (await reader.ReadAsync(ctx))
            {
                return _repositoryMapper.MapComment(reader);
            }

            return null;
        }

        public async Task<List<Comment>> GetCommentsPostedByFollowing(Guid? userId, string topic, long nextPageToken, CancellationToken ctx)
        {
            if (userId == null)
                throw new ArgumentNullException("User id cannot be null when getting comments");

            string sql = @"SELECT Id, UserId, Content, ReplyCount, 
                                      Likes, DtCreated, DtUpdated,
                                      TopicId, Interactions, UserName, Reference
                                      FROM comments 
                                      WHERE UserId = @userId 
                                      AND DtCreated >= @dtFrom 
                                      AND TopicId = @topicId
                                      AND Reference > @nextPageToken
                                      ORDER BY interactions DESC, DtUpdated desc 
                                      LIMIT @commentMax;";


            await using MySqlConnection connection = _mySqlConnectionFactory.CreateUserInfoConnection();
            await connection.OpenAsync(ctx);

            await using MySqlCommand cmd = new MySqlCommand(sql, connection);

            cmd.Parameters.Add("@userId", MySqlDbType.Guid).Value = userId;
            cmd.Parameters.Add("@dtFrom", MySqlDbType.DateTime).Value = Settings.TimeToGetCommentFrom;
            cmd.Parameters.Add("@topicId", MySqlDbType.String).Value = topic;
            cmd.Parameters.Add("@nextPageToken", MySqlDbType.Int64).Value = nextPageToken;
            cmd.Parameters.Add("@commentMax", MySqlDbType.Int32).Value = Settings.CommentMax;

            await using MySqlDataReader reader = await cmd.ExecuteReaderAsync(ctx);

            List<Comment> comments = new List<Comment>();

            while (await reader.ReadAsync(ctx))
            {
                comments.Add(_repositoryMapper.MapComment(reader));
            }

            if (comments == null)
                throw new Exception("Null return when retriving commments");

            return comments;
        }


        public async Task<ImmutableList<Comment>> GetTopCommentsForTodaysTopic(string topic, long? nextPageToken, CancellationToken ctx)
        {
            string sql = @"SELECT Id, UserId, Content, ReplyCount, 
                                  Likes, DtCreated, DtUpdated,
                                  TopicId, Interactions, UserName, Reference
                           FROM comments
                           WHERE TopicId = @topicId AND 
                           Reference > @nextPageToken
                           ORDER BY interactions DESC, DtUpdated desc LIMIT 100;";

            await using MySqlConnection connection = _mySqlConnectionFactory.CreateUserInfoConnection();

            await connection.OpenAsync(ctx);
            await using MySqlCommand cmd = new(sql, connection);

            cmd.Parameters.Add("@topicId", MySqlDbType.VarChar).Value = topic;
            cmd.Parameters.Add("@nextPageToken", MySqlDbType.Int64).Value =
                (nextPageToken == null) ? 0 : nextPageToken;

            await using MySqlDataReader reader = await cmd.ExecuteReaderAsync(ctx);

            List<Comment> comments = new List<Comment>();

            while (await reader.ReadAsync())
            {
                var comment = _repositoryMapper.MapComment(reader);
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
            cmd.Parameters.Add("@id", MySqlDbType.Guid).Value = commentId;

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
            cmd.Parameters.Add("@id", MySqlDbType.Guid).Value = commentId;

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

            cmd.Parameters.Add("@id", MySqlDbType.Guid).Value = comment.Id;
            cmd.Parameters.Add("@userId", MySqlDbType.Guid).Value = comment.UserId;
            cmd.Parameters.Add("@content", MySqlDbType.VarChar).Value = comment.Content;
            cmd.Parameters.Add("@topicId", MySqlDbType.VarChar).Value = comment.TopicId;
            cmd.Parameters.Add("@userName", MySqlDbType.VarChar).Value = comment.UserName;

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
                sql = "UPDATE comments SET likes = likes + 1, dtupdated = NOW()  WHERE id = @id";
                statusMessage = $"{comment.UserId} Successfully liked reply {comment.CommentId}";
            }
            else
            {
                sql = "UPDATE comments SET likes = likes - 1, dtupdated = NOW() WHERE id = @id";
                statusMessage = $"{comment.UserId} Successfully unliked reply {comment.CommentId}";
            }

            await using MySqlConnection connection = _mySqlConnectionFactory.CreateUserInfoConnection();
            await connection.OpenAsync(ctx);

            await using MySqlCommand cmd = new(sql, connection);

            cmd.Parameters.Add("@id", MySqlDbType.Guid).Value = comment.CommentId;

            await cmd.ExecuteNonQueryAsync(ctx);

            return statusMessage;
        }

        public async Task UpdateCommentContentAsync(UpdateCommentDto comment, CancellationToken ctx)
        {
            if (string.IsNullOrEmpty(comment.Content))
                throw new ArgumentNullException("New content is null when attempting to edit content");

            //check if user trying to delete the request is the user who posted it
            if (!await validateUserPostedComment(comment.UserId, comment.Id, ctx))
                throw new ServiceNotAuthenticatedException("User cant edit post as it doesnt belong to them");

            await using MySqlConnection connection = _mySqlConnectionFactory.CreateUserInfoConnection();
            await connection.OpenAsync(ctx);

            await using MySqlCommand cmd = new(@"UPDATE comments SET content = @updatedContent, dtupdated = NOW() WHERE id = @id", connection);
            cmd.Parameters.Add("@updatedContent", MySqlDbType.VarChar).Value = comment.Content.TrimEnd();
            cmd.Parameters.Add("@id", MySqlDbType.Guid).Value = comment.Id;

            await cmd.ExecuteNonQueryAsync(ctx); ;
        }

        //******** Delete Commands ********
        public async Task DeleteCommentAsyncEndpoint(Guid commentId, CancellationToken ctx)
        {
            await using MySqlConnection connection = _mySqlConnectionFactory.CreateUserInfoConnection();
            await connection.OpenAsync(ctx);

            await using MySqlCommand cmd = new(@"DELETE FROM comments WHERE id = @id");
            cmd.Parameters.Add("@id", MySqlDbType.Guid).Value = commentId;

            var result = await cmd.ExecuteNonQueryAsync(ctx);

            //Already in trouble at this point, but atleast we flag and it's now got our attention
            if (result > 0)
                throw new Exception("query resulted in mutiple rows deleted");
            else if (result == 0)
                throw new Exception("query executed but no rows affected");

        }

        //******** private Commands ********
        private async Task<bool> validateUserPostedComment(Guid userId, Guid commentId, CancellationToken ctx)
        {
            await using MySqlConnection connection = _mySqlConnectionFactory.CreateUserInfoConnection();
            await connection.OpenAsync(ctx);

            string sql = @"SELECT COUNT(*) FROM comments WHERE id = @commentId AND userId = @userid";

            await using MySqlCommand cmd = new(sql, connection);
            cmd.Parameters.Add("@commentId", MySqlDbType.Guid).Value = commentId;
            cmd.Parameters.Add("@userid", MySqlDbType.Guid).Value = userId;


            var result = await cmd.ExecuteScalarAsync(ctx);
            var count = result == null ? 0 : Convert.ToInt32(result);

            return count == 1;
        }


    }
}
