using DiscussedApi.Models.Comments;

namespace DiscussedApi.Processing.Comments.ParallelProcess
{
    public interface IProcessCommentsConcurrently
    {
        Task<List<Comment>> GetCommentsConcurrently(List<Guid?> userIds, CancellationToken cancellationToken);
    }
}
