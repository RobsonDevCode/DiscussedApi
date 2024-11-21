using DiscussedApi.Models.Comments;
using Discusseddto.CommentDtos.ReplyDtos;

namespace DiscussedApi.Processing.Replies
{
    public interface IReplyProcessing
    {
        Task PostReplyAsync(PostReplyDto postReplyDto, CancellationToken ctx);
        Task<RepliesWithComment> GetReplysForCommentAsync(Guid commentId, CancellationToken ctx );
        Task DeleteReplyAsync(Guid commentId);
    }
}
