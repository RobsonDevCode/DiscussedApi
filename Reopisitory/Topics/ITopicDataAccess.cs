using DiscussedApi.Models.Topic;

namespace DiscussedApi.Reopisitory.Topics
{
    public interface ITopicDataAccess
    {
        Task<Topic> GenerateTopicForTodayAsync(CancellationToken ctx);
        Task UpdateTopicStatusAsync(CancellationToken ctx);
    }
}
