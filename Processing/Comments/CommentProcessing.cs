using DiscussedApi.Models;
using DiscussedApi.Models.Comments;

namespace DiscussedApi.Processing.Comments
{
    public class CommentProcessing : ICommentProcessing
    {
        public Task<List<Comment>> GetCommentsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<CommentResultDto> PostCommentAsync(User user, Comment comment)
        {
            throw new NotImplementedException();
        }
    }
}
