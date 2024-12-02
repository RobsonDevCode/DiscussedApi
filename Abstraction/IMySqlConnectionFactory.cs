using MySqlConnector;

namespace DiscussedApi.Abstraction
{
    public interface IMySqlConnectionFactory
    {
        MySqlConnection CreateUserInfoConnection();
    }
}
