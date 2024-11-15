using DiscussedApi.Models;
using DiscussedApi.Models.Comments;
using Discusseddto;
using Discusseddto.CommentDtos;
using System.Collections.Immutable;

namespace DiscussedApi.Processing.Comments
{
    public interface ICommentProcessing
    {
       Task<List<Comment>> GetCommentsAsync(Guid userId, string topicName, CancellationToken cancellationToken);
       Task PostCommentAsync(NewCommentDto comment, CancellationToken cancellationToken);
       Task<Comment> LikeOrDislikeCommentAsync(LikeCommentDto commentToEditDto);
       Task DeleteCommentAsync(Guid commentId, CancellationToken cancellationToken);
       Task<ImmutableList<Comment>> GetCommentsWithNoSignInAsync(CancellationToken cancellationToken, string topic);

    }
}
