using DiscussedApi.Models;
using DiscussedApi.Models.Comments;
using Discusseddto;
using Discusseddto.CommentDtos;

namespace DiscussedApi.Processing.Comments
{
    public interface ICommentProcessing
    {
       Task<List<Comment>> GetCommentsAsync(string userId);
       Task PostCommentAsync(NewCommentDto comment);
       Task<Comment> LikeCommentAsync(LikeCommentDto likeCommentDto);
    }
}
