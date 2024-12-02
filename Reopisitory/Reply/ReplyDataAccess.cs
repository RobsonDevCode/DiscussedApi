using Dapper;
using DiscussedApi.Abstraction;
using DiscussedApi.Data.UserComments;
using DiscussedApi.Models.Comments;
using DiscussedApi.Models.Comments.Replies;
using DiscussedApi.Reopisitory.DataMapping;
using Discusseddto.CommentDtos.ReplyDtos;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using Newtonsoft.Json;
using NLog;
using System.Data;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace DiscussedApi.Reopisitory.Replies
{
    public class ReplyDataAccess : IReplyDataAccess
    {
        private readonly NLog.ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly IMySqlConnectionFactory _mySqlConnectionFactory;

        public ReplyDataAccess(IMySqlConnectionFactory mySqlConnectionFactory)
        {
            _mySqlConnectionFactory = mySqlConnectionFactory;
        }

        public async Task<string> DeleteReplyAsync(Guid replyId, CancellationToken ctx)
        {
            var sql = @"DELETE FROM replies WHERE id = @replyId;";

            await using (MySqlConnection connection = _mySqlConnectionFactory.CreateUserInfoConnection())
            {
                await connection.OpenAsync(ctx);

                await using (MySqlCommand cmd = new MySqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@replyId", replyId);

                    int result = await cmd.ExecuteNonQueryAsync(ctx);

                    if (result == 0)
                        throw new KeyNotFoundException("Reply cannot be found or has been deleted");

                    return $"Reply: {replyId} has been deleted";
                }
            }
        }


        public async Task<string> EditReplyLikesAsync(EditReplyLikesDto replyLikesDto, CancellationToken ctx)
        {
            string sql;
            string statusMessage;

            if (replyLikesDto.IsLiked)
            {
                sql = @"UPDATE replies SET likes = likes + 1 WHERE id = @id;";
                statusMessage = $"{replyLikesDto.UserId} Successfully liked reply {replyLikesDto.ReplyId}";
            }
            else
            {
                sql = @"UPDATE replies SET likes = likes - 1 WHERE id = @id;";
                statusMessage = $"{replyLikesDto.UserId} Successfully unliked reply {replyLikesDto.ReplyId}";
            }

            await using (MySqlConnection connection = _mySqlConnectionFactory.CreateUserInfoConnection())
            {
               await connection.OpenAsync(ctx);
                await using (MySqlCommand cmd = new(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@id", replyLikesDto.ReplyId);

                    if (await cmd.ExecuteNonQueryAsync(ctx) == 0)
                        throw new KeyNotFoundException("Reply cannot be found or has been deleted");

                    return statusMessage;
                }
            }
        }

        public async Task<RepliesWithComment> GetRepliesAsync(Guid commentId, CancellationToken ctx)
        {
            string sql = @"SELECT c.Id, c.UserId, c.Content, c.ReplyCount, 
                                      c.Likes, c.DtCreated, c.DtUpdated, c.TopicId, 
                                       c.UserName, c.Reference, c.Interactions
                                   (SELECT JSON_ARRAYAGG(
                                       JSON_OBJECT (
                                           'Id', r.Id,
                                           'CommentId', r.CommentId,
                               			   'UserId', r.UserId,
                                           'UserName', r.UserName,
                                           'Content', r.Content,
                                           'Likes', r.Likes,
                                           'DtCreated', r.DtCreated,
                                           'DtUpdated', r.DtUpdated
                                       ) 
                                   )
                                   FROM Replies r 
                                   WHERE r.CommentId = c.Id) AS Replies
                               FROM Comments c
                               LEFT JOIN Replies r ON c.Id = r.CommentId
                               WHERE r.CommentId = @commentId
                               LIMIT 1;
                               ";

            await using MySqlConnection connection = _mySqlConnectionFactory.CreateUserInfoConnection();
            await connection.OpenAsync(ctx);

            await using MySqlCommand cmd = new(sql, connection);
            cmd.Parameters.AddWithValue("@commentId", commentId);
            cmd.CommandTimeout = 30;

            await using var reader = await cmd.ExecuteReaderAsync(ctx);
            RepliesWithComment repliesWithComment = new RepliesWithComment();

            while (await reader.ReadAsync(ctx))
            {
                repliesWithComment = new RepliesWithComment()
                {
                    Comment = RepositoryMappers.MapComment(reader),
                    Replies = RepositoryMappers.MapReplies(reader)
                };

            }

            return repliesWithComment;
        }

        public async Task PostReply(PostReplyDto reply, CancellationToken ctx)
        {
            string sql = @"INSERT INTO Replies 
                           (Id, CommentId, UserId, UserName, Content, Likes, DtCreated, DtUpdated) 
                           VALUES 
                           (@Id, @CommentId, @UserId, @UserName, @Content, @Likes, @DtCreated, @DtUpdated)";

            await using MySqlConnection connection = _mySqlConnectionFactory.CreateUserInfoConnection();
            await connection.OpenAsync(ctx);

            await using MySqlCommand cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Id", reply.Id);
            cmd.Parameters.AddWithValue("@CommentId", reply.CommentId);
            cmd.Parameters.AddWithValue("@UserId", reply.UserId);
            cmd.Parameters.AddWithValue("@UserName", reply.UserName);
            cmd.Parameters.AddWithValue("@Content", reply.Content);
            cmd.Parameters.AddWithValue("@Likes", reply.Likes);
            cmd.Parameters.AddWithValue("@DtCreated", DateTime.UtcNow);
            cmd.Parameters.AddWithValue("@DtUpdated", DateTime.UtcNow);

            int rowsAffected = await cmd.ExecuteNonQueryAsync(ctx);

            if (rowsAffected == 0)
                throw new Exception("Query Excecuted but no updated was made");
        }
        
        private List<Reply> MapReplies(MySqlDataReader reader)
        {
            var repliesJson = reader["Replies"]?.ToString();

            if (string.IsNullOrEmpty(repliesJson))
                throw new Exception("Error getting json from reader, when reading replies");

            List<Reply>? result = JsonConvert.DeserializeObject<List<Reply>>(repliesJson);

            if (result == null)
                throw new JsonException("Error Deserializing replies json");

            return result;
        }
    }
}
