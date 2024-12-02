using DiscussedApi.Configuration;
using MySqlConnector;

namespace DiscussedApi.Abstraction
{
    public class MySqlConnectionFactory : IMySqlConnectionFactory
    {
        public MySqlConnection CreateUserInfoConnection()
        {
            return new MySqlConnection(Settings.ConnectionString.UserInfo);
        }
    }
}
