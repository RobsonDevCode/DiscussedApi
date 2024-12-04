using DiscussedApi.Abstraction;
using DiscussedApi.Models.Comments.Replies;
using DiscussedApi.Reopisitory.DataMapping;
using Discusseddto.CommentDtos.ReplyDtos;
using MySqlConnector;

namespace DiscussedApi.Reopisitory.Replies
{
    public class ReplyDataAccess : IReplyDataAccess
    {
        private readonly IMySqlConnectionFactory _mySqlConnectionFactory;
        private readonly IRepositoryMapper _repositoryMapper;
        public ReplyDataAccess(IMySqlConnectionFactory mySqlConnectionFactory, IRepositoryMapper repositoryMapper)
        {
            _mySqlConnectionFactory = mySqlConnectionFactory;
            _repositoryMapper = repositoryMapper;
        }

        public async Task<string> DeleteReplyAsync(Guid replyId, CancellationToken ctx)
        {
            string sql = @"DELETE FROM replies WHERE id = @replyId;";

            await using (MySqlConnection connection = _mySqlConnectionFactory.CreateUserInfoConnection())
            {
                await connection.OpenAsync(ctx);

                await using (MySqlCommand cmd = new MySqlCommand(sql, connection))
                {
                    cmd.Parameters.Add("@replyId", MySqlDbType.Guid).Value = replyId;

                    int result = await cmd.ExecuteNonQueryAsync(ctx);

                    if (result == 0)
                        throw new KeyNotFoundException("Reply cannot be found or has been deleted");

                    return $"Reply: {replyId} has been deleted";
                }
            }
        }

        public async Task<string> EditReplyContentAsync(EditReplyContentDto editedLikes, CancellationToken ctx)
        {
            await using MySqlConnection connection = _mySqlConnectionFactory.CreateUserInfoConnection();
            await connection.OpenAsync(ctx);

            await using MySqlCommand cmd = new(@"UPDATE replies SET content = @content WHERE id = @id", connection);
            cmd.Parameters.Add("@content", MySqlDbType.VarChar).Value = editedLikes.Content;
            cmd.Parameters.Add("@id", MySqlDbType.Guid).Value = editedLikes.ReplyId;

            int result = await cmd.ExecuteNonQueryAsync(ctx);
            if (result == 0)
                throw new KeyNotFoundException("Reply cannot be found or has been deleted");

            return $"Reply: {editedLikes.ReplyId} content updated";
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
                    cmd.Parameters.Add("@id", MySqlDbType.Guid).Value = replyLikesDto.ReplyId;

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
            cmd.Parameters.Add("@commentId", MySqlDbType.Guid).Value = commentId;
            cmd.CommandTimeout = 30;

            await using var reader = await cmd.ExecuteReaderAsync(ctx);
            RepliesWithComment repliesWithComment = new RepliesWithComment();

            while (await reader.ReadAsync(ctx))
            {
                repliesWithComment = new RepliesWithComment()
                {
                    Comment = _repositoryMapper.MapComment(reader),
                    Replies = _repositoryMapper.MapReplies(reader)
                };

            }

            return repliesWithComment;
        }

        public async Task PostReply(PostReplyDto reply, CancellationToken ctx)
        {
            string sql = @"INSERT INTO Replies 
                           (Id, CommentId, UserId, UserName, Content, Likes, DtCreated, DtUpdated) 
                           VALUES 
                           (@Id, @CommentId, @UserId, @UserName, @Content, 0, NOW(), NOW())";

            await using MySqlConnection connection = _mySqlConnectionFactory.CreateUserInfoConnection();
            await connection.OpenAsync(ctx);

            await using MySqlCommand cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.Add("@Id", MySqlDbType.Guid).Value = reply.Id;
            cmd.Parameters.Add("@CommentId", MySqlDbType.Guid).Value = reply.CommentId;
            cmd.Parameters.Add("@UserId", MySqlDbType.Guid).Value = reply.UserId;
            cmd.Parameters.Add("@UserName", MySqlDbType.VarChar).Value = reply.UserName;
            cmd.Parameters.Add("@Content", MySqlDbType.VarChar).Value = reply.Content;

            int rowsAffected = await cmd.ExecuteNonQueryAsync(ctx);

            if (rowsAffected == 0)
                throw new Exception("Query Excecuted but no updated was made");
        }

       
    }
}
