using DiscussedApi.Models.Comments;
using DiscussedApi.Models.Comments.Replies;
using MySqlConnector;
using Newtonsoft.Json;
using System.ComponentModel.Design;

namespace DiscussedApi.Reopisitory.DataMapping
{
    public static class RepositoryMappers
    {
        public static List<Reply> MapReplies(MySqlDataReader reader)
        {
            var repliesJson = reader["Replies"]?.ToString();

            if (string.IsNullOrEmpty(repliesJson))
                throw new Exception("Error getting json from reader, when reading replies");

            List<Reply>? result = JsonConvert.DeserializeObject<List<Reply>>(repliesJson);

            if (result == null)
                throw new JsonException("Error Deserializing replies json");

            return result;
        }

        public static Comment MapComment(MySqlDataReader reader)
        {
            return new Comment()
            {
                Id = Guid.TryParse(reader["Id"].ToString(), out Guid commentId) ? commentId : Guid.Empty,
                UserId = Guid.TryParse(reader["UserId"].ToString(), out Guid userId) ? userId : Guid.Empty,
                Content = reader.GetString("Content"),
                ReplyCount = reader.GetInt32("ReplyCount"),
                Likes = reader.GetInt32("Likes"),
                DtCreated = reader.GetDateTime("DtCreated"),
                DtUpdated = reader.GetDateTime("DtUpdated"),
                TopicId = reader.GetString("TopicId"),
                UserName = reader.GetString("UserName"),
                Reference = reader.GetInt32("Refernce"),
                Interactions = reader.GetInt32("Interactions")
            };
        }
    }
}
