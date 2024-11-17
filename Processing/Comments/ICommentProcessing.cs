using DiscussedApi.Models;
using DiscussedApi.Models.Comments;
using Discusseddto;
using Discusseddto.CommentDtos;
using System.Collections.Immutable;

namespace DiscussedApi.Processing.Comments
{
    public interface ICommentProcessing
    {
       Task<List<Comment>> GetCommentsAsync(Guid userId, string topicName, CancellationToken ctx);
       Task PostCommentAsync(NewCommentDto comment, CancellationToken ctx);
       Task<Comment> LikeOrDislikeCommentAsync(LikeCommentDto commentToEditDto);
       Task DeleteCommentAsync(Guid commentId, CancellationToken ctx);
       Task<ImmutableList<Comment>> GetCommentsWithNoSignInAsync(CancellationToken ctx, string topic);
        Task<Comment> EditCommentContentAsync(UpdateCommentDto updateComment, CancellationToken ctx);
    }
}
