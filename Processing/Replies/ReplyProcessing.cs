using DiscussedApi.Models.Comments;
using DiscussedApi.Reopisitory.Replies;
using Discusseddto.CommentDtos.ReplyDtos;
using NLog;

namespace DiscussedApi.Processing.Replies
{
    public class ReplyProcessing : IReplyProcessing
    {
        private readonly NLog.ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly IReplyDataAccess _replyDataAccess;
        public ReplyProcessing(IReplyDataAccess replyDataAccess)
        {
            _replyDataAccess = replyDataAccess;
        }
        public async Task<RepliesWithComment> GetReplysForCommentAsync(Guid commentId, CancellationToken ctx)
        {
            try
            {
                return await _replyDataAccess.GetRepliesAsync(commentId, ctx);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                throw;
            }
        }

        public async Task PostReplyAsync(PostReplyDto postReplyDto, CancellationToken ctx)
        {
            try
            {
                Reply reply = new Reply()
                {
                    Id = postReplyDto.Id,
                    UserName = postReplyDto.UserName,
                    UserId = postReplyDto.UserId,
                    CommentId = postReplyDto.CommentId,
                    Content = postReplyDto.ReplyContent,
                    DtCreated = DateTime.UtcNow,
                    DtUpdated = DateTime.UtcNow,
                    Likes = 0,
                };

                await _replyDataAccess.PostReply(reply, ctx);
            }

            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                throw;
            }
        }
        public Task DeleteReplyAsync(Guid commentId)
        {
            throw new NotImplementedException();
        }
    }
}
