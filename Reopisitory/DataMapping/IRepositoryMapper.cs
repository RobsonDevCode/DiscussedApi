using DiscussedApi.Models.Comments;
using DiscussedApi.Models.Comments.Replies;
using DiscussedApi.Models.Topic;
using MySqlConnector;

namespace DiscussedApi.Reopisitory.DataMapping
{
    public interface IRepositoryMapper
    {
        List<Reply> MapReplies(MySqlDataReader reader);
        Comment MapComment(MySqlDataReader reader);
        Topic MapTopic(MySqlDataReader reader);
    }
}
