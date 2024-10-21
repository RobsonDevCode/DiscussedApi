using DiscussedApi.Models;
using DiscussedApi.Models.Comments;

namespace DiscussedApi.Processing.Comments
{
    public interface ICommentProcessing
    {
       Task<List<Comment>> GetCommentsAsync();
       Task<CommentResultDto> PostCommentAsync(User user, Comment comment);
    }
}
