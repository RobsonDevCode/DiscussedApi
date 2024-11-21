using DiscussedApi.Data.UserComments;
using DiscussedApi.Models.Comments;
using Discusseddto.CommentDtos.ReplyDtos;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace DiscussedApi.Reopisitory.Replies
{
    public class ReplyDataAccess : IReplyDataAccess
    {
        private readonly NLog.ILogger _logger = LogManager.GetCurrentClassLogger();

        public async Task<RepliesWithComment> GetRepliesAsync(Guid commentId, CancellationToken ctx)
        {
            try
            {
                using (var connection = new CommentsDBContext())
                {
                    var repliesWithComment = await connection.Comments.
                        Join(connection.Replies.Where(r => r.CommentId == commentId),
                        comment => comment.Id,
                        reply => reply.CommentId,
                        (comment, reply) => new RepliesWithComment{
                            Comment = comment,
                            Replies = connection.Replies.Where(r => r.CommentId == commentId).ToList(),
                        }).FirstOrDefaultAsync();

                    if (repliesWithComment == null)
                    {
                        _logger.Error($"replies for comment {commentId} returned null");
                        throw new Exception($"replies for comment {commentId} returned null");
                    }    

                    return repliesWithComment;
                }
            }
            catch (Exception ex)
            {
                throw new NotImplementedException();

            }
        }

        public async Task PostReply(Reply postReply, CancellationToken ctx)
        {
            try
            {
                using (var connection = new CommentsDBContext())
                {
                    var post = await connection.Replies.AddAsync(postReply, ctx);

                    if (await connection.SaveChangesAsync(ctx) == 0)
                        throw new Exception("Query Excecuted by no rows were affected");

                }
            }
            catch(Exception ex)
            {
                _logger.Error(ex);
                throw;
            }
        }
    }
}
