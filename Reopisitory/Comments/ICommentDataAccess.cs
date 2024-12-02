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
        Task<List<Comment>> GetCommentsPostedByFollowing(Guid? userId, string topic, long nextPageToken, CancellationToken ctx);
        Task<ImmutableList<Comment>> GetTopCommentsForTodaysTopic(string topic, long? nextPageToken ,CancellationToken ctx);

        Task<Comment?> GetComment(Guid commentId, CancellationToken ctx);
        //********** POST/PUT/PATCH **********
        Task PostCommentAsync(NewCommentDto comment, CancellationToken ctx);
        Task UpdateCommentContentAsync(UpdateCommentDto comment, CancellationToken ctx);
        Task<string> UpdateCommentLikesAsync(LikeCommentDto likedComment, CancellationToken ctx);

        //********** DELETE **********
        Task DeleteCommentAsyncEndpoint(Guid userId, CancellationToken ctx);

        //********** VALIDATION **********
        Task<bool> IsCommentValid(Guid? commentId);
        Task<bool> IsCommentValid(Guid? commentId, CancellationToken ctx);

    }
}
