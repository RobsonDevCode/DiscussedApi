using DiscussedApi.Abstraction;
using DiscussedApi.Configuration;
using DiscussedApi.Models.Topic;
using DiscussedApi.Reopisitory.DataMapping;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using NLog;

namespace DiscussedApi.Reopisitory.Topics
{
    public class TopicDataAccess : ITopicDataAccess
    {
        private readonly NLog.ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly IMySqlConnectionFactory _mySqlConnectionFactory;
        private readonly IRepositoryMapper _repositoryMapper;
        public TopicDataAccess(IMySqlConnectionFactory mySqlConnectionFactory, IRepositoryMapper repositoryMapper)
        {
            _mySqlConnectionFactory = mySqlConnectionFactory;
            _repositoryMapper = repositoryMapper;
        }

        public async Task<Topic> GenerateTopicForTodayAsync(CancellationToken ctx)
        {
            await using MySqlConnection connection = _mySqlConnectionFactory.CreateUserInfoConnection();
            await connection.OpenAsync(ctx);

            await using MySqlCommand cmd = new(@"SELECT TopicId, Name, DtCreated, Category, IsActive, Likes  FROM topics WHERE isActive = true LIMIT 1", connection);

            await using MySqlDataReader reader = await cmd.ExecuteReaderAsync(ctx);

            Topic topic = new Topic();
            while (await reader.ReadAsync())
            {
                topic = _repositoryMapper.MapTopic(reader);
            }

            if (topic == null)
                throw new Exception("Failed when trying to get active topic");

            _logger.Info($"Topic {topic.Name ?? ""} has been set on {DateTime.UtcNow}");

            return topic;

        }

        //TODO move into a stored procedure and have it on some kind of scheduled task
        public async Task UpdateTopicStatusAsync(CancellationToken ctx)
        {
            await using MySqlConnection connection = _mySqlConnectionFactory.CreateUserInfoConnection();
            await connection.OpenAsync(ctx);

         //   await SetCurrentTopicToInActive(connection, ctx);
            await SetNewActiveTopic(connection, ctx);
        }

        private async Task SetCurrentTopicToInActive(MySqlConnection connection, CancellationToken ctx)
        {
            await using MySqlCommand cmd = new MySqlCommand(@"UPDATE topics SET IsActive = 0 WHERE IsActive = 1;", connection);
            var result = await cmd.ExecuteNonQueryAsync();

            if (result == 0)
                throw new Exception("Deactivate Query Excecuted but no value changed");
        }

        private async Task SetNewActiveTopic(MySqlConnection connection, CancellationToken ctx)
        {
            await using MySqlCommand cmd = new MySqlCommand(@"UPDATE topics SET IsActive = 1 WHERE DtCreated = @dateToGetTopic;", connection);
            cmd.Parameters.Add(@"dateToGetTopic", MySqlDbType.DateTime).Value = Settings.DateToGetTopic;

            var result = await cmd.ExecuteNonQueryAsync();
            if (result == 0)
                throw new Exception("Set to Is ActiveQuery Excecuted but no value changed");
        }

    }
}
