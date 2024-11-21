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
        //********** GET **********
        Task<List<Comment>> GetCommentsPostedByFollowing(Guid? userId, string topic, CancellationToken ctx);
        Task<ImmutableList<Comment>> GetTopCommentsForTodaysTopic(string topic, CancellationToken ctx);
        Task<ImmutableList<Comment>> GetTopCommentsForTodaysTopic(string topic, long? nextPageToken ,CancellationToken ctx);

        //********** POST/PUT/PATCH **********
        Task PostCommentAsync(Comment comment, CancellationToken ctx);
        Task<Comment> UpdateCommentContentAsync(UpdateCommentDto comment, CancellationToken ctx);
        Task<Comment> UpdateCommentLikesAsync(LikeCommentDto likedComment);

        //********** DELETE **********
        Task DeleteCommentAsyncEndpoint(Guid userId, CancellationToken ctx);

        //********** VALIDATION **********
        Task<bool> IsCommentValid(Guid? commentId);
        Task<bool> IsCommentValid(Guid? commentId, CancellationToken ctx);

    }
}
