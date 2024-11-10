using DiscussedApi.Models.Comments;
using DiscussedApi.Models.Profiles;
using DiscussedApi.Models.UserInfo;
using Discusseddto;
using Discusseddto.CommentDtos;

namespace DiscussedApi.Reopisitory.Comments
{
    public interface ICommentDataAccess
    {
        Task<List<Comment>> GetTopLikedCommentsAsyncEndPoint(List<Guid?> followings);
        Task<List<Comment>> GetCommentsPostedByFollowing(Guid? userId, CancellationToken cancellationToken);
        Task<List<Comment>> GetCommentsPostedByFollowing(List<Guid?> userId);
        Task DeleteCommentAsyncEndpoint(User user);
        Task PostCommentAsync(Comment comment, CancellationToken cancellationToken);
        Task<Dictionary<Comment, List<Reply>>> UpdateComment(User user, Comment comment);
        Task<Comment> UpdateCommentLikesAsync(LikeCommentDto likedComment);
        Task<bool> IsCommentValid(Guid? commentId);
        Task<List<Comment>> GetCommentsForNewUserAsync(string userId);
    }
}
