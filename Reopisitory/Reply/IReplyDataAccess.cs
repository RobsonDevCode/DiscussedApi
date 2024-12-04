using DiscussedApi.Models.Comments.Replies;
using Discusseddto.CommentDtos.ReplyDtos;

namespace DiscussedApi.Reopisitory.Replies
{
    public interface IReplyDataAccess
    {
        Task PostReply(PostReplyDto postReply, CancellationToken ctx);
        Task<RepliesWithComment> GetRepliesAsync(Guid commentId, CancellationToken ctx);
        Task<string> EditReplyLikesAsync(EditReplyLikesDto replyLikesDto, CancellationToken ctx);
        Task<string> EditReplyContentAsync(EditReplyContentDto editedLikes, CancellationToken ctx);
        Task<string> DeleteReplyAsync(Guid replyId, CancellationToken ctx);
    }
}
