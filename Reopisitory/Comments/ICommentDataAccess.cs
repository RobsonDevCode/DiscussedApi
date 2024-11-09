using DiscussedApi.Models.Comments;
using DiscussedApi.Models.Profiles;
using DiscussedApi.Models.UserInfo;
using Discusseddto;
using Discusseddto.CommentDtos;

namespace DiscussedApi.Reopisitory.Comments
{
    public interface ICommentDataAccess
    {
        Task<Dictionary<List<Comment>, List<Reply>>> GetTopLikedCommentsAsyncEndPoint(List<Following> followings);
        Task<List<Comment>> GetCommentsPostedByFollowing(List<Guid?> userId);
        Task DeleteCommentAsyncEndpoint(User user);
        Task PostCommentAsync(Comment comment);
        Task<Dictionary<Comment, List<Reply>>> UpdateComment(User user, Comment comment);
        Task<Comment> UpdateCommentLikesAsync(LikeCommentDto likedComment);
        Task<bool> IsCommentValid(Guid? commentId);
        Task<List<Comment>> GetCommentsForNewUserAsync(string userId);
    }
}
