using DiscussedApi.Models.Topic;
using DiscussedApi.Reopisitory.Topics;
using NLog;

namespace DiscussedApi.Processing.Topics
{
    public class TopicProcessing : ITopicProcessing
    {
        private readonly ITopicDataAccess _topicDataAccess;
        private readonly NLog.ILogger _logger = LogManager.GetCurrentClassLogger();
        public TopicProcessing(ITopicDataAccess topicDataAccess)
        {
            _topicDataAccess = topicDataAccess;
        }
        /// <summary>
        /// GetTopicAsync: Systematic api call made to generate the new Topic at the start of each day
        /// </summary>
        /// <returns></returns>
        public async Task<Topic> GetTopicAsync(CancellationToken ctx)
        {
          //  await _topicDataAccess.UpdateTopicStatusAsync(ctx);

            return await _topicDataAccess.GenerateTopicForTodayAsync(ctx);
        }
    }
}
