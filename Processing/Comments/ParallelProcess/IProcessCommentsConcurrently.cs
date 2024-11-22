using DiscussedApi.Models.Comments;
using DiscussedApi.Models.Topic;

namespace DiscussedApi.Processing.Comments.ParallelProcess
{
    public interface IProcessCommentsConcurrently
    {
        Task<List<Comment>> GetCommentsConcurrently(List<Guid?> userIds,string topic, long? nextPageToken ,CancellationToken ctx);
    }
}
