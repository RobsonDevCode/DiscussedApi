using DiscussedApi.Models;
using DiscussedApi.Models.Comments;

namespace DiscussedApi.Reopisitory
{
    public interface ICommentDataAccess
    {
        Task<Dictionary<Comment, List<Reply>>> GetCommentsAsyncEndPoint();
        Task<Comment> DeleteCommentAsyncEndpoint(User user);
        Task<Comment> PostComment(User user, Comment comment);
        Task<Comment> UpdateComment(User user, Comment comment);
    }
}
