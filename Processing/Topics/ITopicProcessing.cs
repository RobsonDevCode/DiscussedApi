using DiscussedApi.Models.Topic;

namespace DiscussedApi.Processing.Topics
{
    public interface ITopicProcessing
    {
        public Task<Topic> GetTopicAsync(CancellationToken ctx);

    }
}
