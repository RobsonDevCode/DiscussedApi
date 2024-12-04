using DiscussedApi.Models.Comments.Replies;
using Discusseddto.CommentDtos.ReplyDtos;

namespace DiscussedApi.Processing.Replies
{
    public interface IReplyProcessing
    {
        Task PostReplyAsync(PostReplyDto postReplyDto, CancellationToken ctx);
        Task<RepliesWithComment> GetReplysForCommentAsync(Guid commentId, CancellationToken ctx );
        Task<string> DeleteReplyAsync(Guid replyId, Guid commmentId,string userId, CancellationToken ctx);
        Task<string> EditReplyLikesAsync(EditReplyLikesDto replyLikesDto, CancellationToken ctx);
        Task<string> EditReplyContent(EditReplyContentDto editedReply, CancellationToken ctx);

    }
}
