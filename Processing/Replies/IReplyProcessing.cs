using DiscussedApi.Models.Comments;
using Discusseddto.CommentDtos.ReplyDtos;

namespace DiscussedApi.Processing.Replies
{
    public interface IReplyProcessing
    {
        Task PostReplyAsync(PostReplyDto postReplyDto);
        Task<Dictionary<Comment, List<Reply>>> GetReplysForCommentAsync(Guid commentId);
        Task DeleteReplyAsync(Guid commentId);
    }
}
