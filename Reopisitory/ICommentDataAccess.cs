﻿using DiscussedApi.Models;
using DiscussedApi.Models.Comments;
using Discusseddto;
using Discusseddto.CommentDtos;

namespace DiscussedApi.Reopisitory
{
    public interface ICommentDataAccess
    {
        Task<Dictionary<List<Comment>, List<Reply>>> GetCommentsAsyncEndPoint();
        Task DeleteCommentAsyncEndpoint(User user);
        Task PostCommentAsync(Comment comment);
        Task<Dictionary<Comment, List<Reply>>> UpdateComment(User user, Comment comment);

        Task<Comment> UpdateCommentLikesAsync(LikeCommentDto likedComment);
        Task<bool> IsCommentValid(Guid? commentId);
    }
}
