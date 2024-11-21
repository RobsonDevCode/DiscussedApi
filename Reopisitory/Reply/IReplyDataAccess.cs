using DiscussedApi.Models.Comments;
using Discusseddto.CommentDtos.ReplyDtos;

namespace DiscussedApi.Reopisitory.Replies
{
    public interface IReplyDataAccess
    {
        Task PostReply(Reply postReply, CancellationToken ctx);
        Task<RepliesWithComment> GetRepliesAsync(Guid commentId, CancellationToken ctx);
    }
}
