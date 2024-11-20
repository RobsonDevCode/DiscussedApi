using DiscussedApi.Models.Comments;
using Discusseddto.CommentDtos.ReplyDtos;
using NLog;

namespace DiscussedApi.Processing.Replies
{
    public class ReplyProcessing : IReplyProcessing
    {
        private readonly NLog.ILogger _logger = LogManager.GetCurrentClassLogger();

        public ReplyProcessing()
        {
            
        }
        public Task<Dictionary<Comment, List<Reply>>> GetReplysForCommentAsync(Guid commentId)
        {
            throw new NotImplementedException();
        }

        public async Task PostReplyAsync(PostReplyDto postReplyDto)
        {
            if (postReplyDto == null)
            {
                throw new ArgumentNullException("Reply cannot be null");
            }
            
        }
        public Task DeleteReplyAsync(Guid commentId)
        {
            throw new NotImplementedException();
        }
    }
}
