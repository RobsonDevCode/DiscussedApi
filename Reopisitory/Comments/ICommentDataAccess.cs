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
        Task<List<Comment>> GetCommentsPostedByFollowing(Guid? userId, string topic, CancellationToken ctx);
        Task<List<Comment>> GetCommentsForNewUserAsync(Guid? userId, string topic);
        Task<ImmutableList<Comment>> GetTopCommentsForTodaysTopic(string topic);


        Task PostCommentAsync(Comment comment, CancellationToken ctx);
        Task<Comment> UpdateCommentContentAsync(UpdateCommentDto comment, CancellationToken ctx);
        Task<Comment> UpdateCommentLikesAsync(LikeCommentDto likedComment);

        Task DeleteCommentAsyncEndpoint(Guid userId, CancellationToken ctx);
        Task<bool> IsCommentValid(Guid? commentId);
    }
}
