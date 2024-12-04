using DiscussedApi.Models.Comments.Replies;
using DiscussedApi.Models.UserInfo;
using DiscussedApi.Reopisitory.Comments;
using DiscussedApi.Reopisitory.Replies;
using Discusseddto.CommentDtos.ReplyDtos;
using Microsoft.AspNetCore.Identity;
using NLog;

namespace DiscussedApi.Processing.Replies
{
    public class ReplyProcessing : IReplyProcessing
    {
        private readonly NLog.ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly IReplyDataAccess _replyDataAccess;
        private readonly UserManager<User> _userManager;
        private readonly ICommentDataAccess _commentDataAccess;
        public ReplyProcessing(IReplyDataAccess replyDataAccess,
            UserManager<User> userManager,
            ICommentDataAccess commentDataAccess)
        {
            _replyDataAccess = replyDataAccess;
            _userManager = userManager;
            _commentDataAccess = commentDataAccess;
        }
        public async Task<RepliesWithComment> GetReplysForCommentAsync(Guid commentId, CancellationToken ctx)
        {
            return await _replyDataAccess.GetRepliesAsync(commentId, ctx);
        }

        public async Task PostReplyAsync(PostReplyDto postReplyDto, CancellationToken ctx)
        {
            await _replyDataAccess.PostReply(postReplyDto, ctx);
        }

        public async Task<string> EditReplyLikesAsync(EditReplyLikesDto replyLikesDto, CancellationToken ctx)
        {
            var statusMessage = await _replyDataAccess.EditReplyLikesAsync(replyLikesDto, ctx);

            if (string.IsNullOrEmpty(statusMessage))
                throw new KeyNotFoundException("Reply cannot be found or has been deleted!");

            return statusMessage;
        }

        public async Task<string> EditReplyContent(EditReplyContentDto editedReply, CancellationToken ctx)
        {
            var result = await _replyDataAccess.EditReplyContentAsync(editedReply, ctx);
            if (string.IsNullOrWhiteSpace(result))
                throw new Exception("Error trying to return updated reply content");

            return result;
        }

        public async Task<string> DeleteReplyAsync(Guid replyId, Guid commentId, string userId, CancellationToken ctx)
        {
            if (!await _commentDataAccess.IsCommentValid(commentId))
                throw new KeyNotFoundException("Comment reply is attempting respond to cannot be found or has been deleted");

            if (await _userManager.FindByIdAsync(userId) == null)
                throw new UnauthorizedAccessException("No account found when trying to reply");

            return await _replyDataAccess.DeleteReplyAsync(replyId, ctx);

        }
    }
}
