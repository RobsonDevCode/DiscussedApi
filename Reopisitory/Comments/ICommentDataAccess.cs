using DiscussedApi.Models.Comments;
using DiscussedApi.Models.Profiles;
using DiscussedApi.Models.UserInfo;
using Discusseddto;
using Discusseddto.CommentDtos;
using System.Collections.Immutable;

namespace DiscussedApi.Reopisitory.Comments
{
    public interface ICommentDataAccess
    {
        Task<List<Comment>> GetCommentsPostedByFollowing(Guid? userId, string topic, CancellationToken cancellationToken);

        Task<ImmutableList<Comment>> GetTopCommentsForTodaysTopic(string topic);

        Task PostCommentAsync(Comment comment, CancellationToken cancellationToken);
        Task<Dictionary<Comment, List<Reply>>> UpdateComment(User user, Comment comment);
        Task<Comment> UpdateCommentLikesAsync(LikeCommentDto likedComment);

        Task DeleteCommentAsyncEndpoint(Guid userId, CancellationToken cancellationToken);

        Task<bool> IsCommentValid(Guid? commentId);
        Task<List<Comment>> GetCommentsForNewUserAsync(Guid? userId, string topic);

    }
}
