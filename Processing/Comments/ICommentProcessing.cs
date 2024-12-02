using DiscussedApi.Models;
using DiscussedApi.Models.Comments;
using Discusseddto;
using Discusseddto.CommentDtos;
using System.Collections.Immutable;

namespace DiscussedApi.Processing.Comments
{
    public interface ICommentProcessing
    {
       Task<List<Comment>> GetFollowingCommentsAsync(Guid userId, string topicName,long? nextPageToken ,CancellationToken ctx);
       Task PostCommentAsync(NewCommentDto comment, CancellationToken ctx);
       Task<string> LikeOrDislikeCommentAsync(LikeCommentDto commentToEditDto, CancellationToken ctx);
       Task DeleteCommentAsync(Guid commentId, CancellationToken ctx);
       Task<ImmutableList<Comment>> GetTopCommentsAsync(string topic, long? nextPageToken ,CancellationToken ctx);
       Task<Comment?> EditCommentContentAsync(UpdateCommentDto updateComment, CancellationToken ctx);
        
    }
}
