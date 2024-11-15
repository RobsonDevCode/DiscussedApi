using DiscussedApi.Configuration;
using DiscussedApi.Data.Topics;
using DiscussedApi.Models.Topic;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace DiscussedApi.Reopisitory.Topics
{
    public class TopicDataAccess : ITopicDataAccess
    {
        private readonly NLog.ILogger _logger = LogManager.GetCurrentClassLogger();
        public async Task<Topic> GenerateTopicForTodayAsync()
        {
            try
            {
                using (var topicDb = new TopicDBContext())
                {
                    var topic = await topicDb.Topics.SingleOrDefaultAsync(t => t.IsActive);

                    _logger.Info($"Topic {topic.Name ?? ""} has been set on {DateTime.UtcNow}");

                    return (topic == null) ? throw new Exception("Error topic returned null, no topic has been set active") : topic;
                }
            }
            catch(Exception ex)
            {
                _logger.Error(ex, ex.Message);
                throw;
            }
        }

        //TODO move into a stored procedure and have it on some kind of scheduled task
        public async Task UpdateTopicStatusAsync()
        {
            try
            {
                using (var topicDb = new TopicDBContext())
                {
                    //set the current result to inactive 
                    var result = await topicDb.Topics
                                              .Where(x => x.IsActive)
                                              .ExecuteUpdateAsync(x => x
                                              .SetProperty(t => t.IsActive, false));
                    if (result == 0)
                        throw new Exception("Deactivate Query Excecuted but no value changed");

                    //set the new topic by day as active
                    result = await topicDb.Topics
                                         .Where(t => t.DtCreated == Settings.DateToGetTopic)
                                         .ExecuteUpdateAsync(x => x.SetProperty(t => t.IsActive, true));

                    if (result == 0)
                        throw new Exception("Set to Is ActiveQuery Excecuted but no value changed");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                throw;
            }
        }
    }
}
