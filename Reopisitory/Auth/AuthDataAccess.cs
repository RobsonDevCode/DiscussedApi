using DiscussedApi.Abstraction;
using DiscussedApi.Models.Auth;
using Discusseddto.Auth;
using MySqlConnector;

namespace DiscussedApi.Reopisitory.Auth
{
    public class AuthDataAccess : IAuthDataAccess
    {
        private readonly IMySqlConnectionFactory _mySQLConnectionFactory;
        public AuthDataAccess(IMySqlConnectionFactory mySqlConnectionFactory)
        {
            _mySQLConnectionFactory = mySqlConnectionFactory;
        }

        public async Task<RefreshToken?> GetTokenById(string tokenSent)
        {
            await using MySqlConnection connection = _mySQLConnectionFactory.CreateUserInfoConnection();
            await connection.OpenAsync();
            await using MySqlCommand cmd = new MySqlCommand(@"SELECT username, 
	                                                          token, 
                                                              expiresOnUtc FROM refreshTokens 
                                                              WHERE token = @refToken LIMIT 1", connection);

            cmd.Parameters.Add("@refToken", MySqlDbType.VarChar).Value = tokenSent;

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new RefreshToken()
                {
                    Username = reader.GetString("username"),
                    Token = reader.GetString("token"),
                    ExpiresOnUtc = reader.GetDateTime("expiresOnUtc")
                };
            }

            return null;
        }

        public async Task StoreRefreshToken(RefreshToken token)
        {
            await using MySqlConnection connection = _mySQLConnectionFactory.CreateUserInfoConnection();
            await connection.OpenAsync();

            await using MySqlCommand cmd = new(@"INSERT INTO refreshTokens(username, token,  expiresOnUtc)
                                                VALUES(@username, @token, @expire)", connection);

            cmd.Parameters.Add("@username", MySqlDbType.VarChar).Value = token.Username;
            cmd.Parameters.Add("@token", MySqlDbType.VarChar).Value = token.Token;
            cmd.Parameters.Add("@expire", MySqlDbType.DateTime).Value = token.ExpiresOnUtc;


            var result = await cmd.ExecuteNonQueryAsync();

            if (result == 0)
                throw new Exception("Query excecuted but no change was made!");
        }

        public async Task StoreKeyAndIv(EncyrptionCredentialsDto encyrptionCredentials)
        {
            await using MySqlConnection connection = _mySQLConnectionFactory.CreateUserInfoConnection();
            await connection.OpenAsync();

            await using MySqlCommand cmd = new(@"INSERT INTO keyandiv(id, aes_key, iv) 
                                                 VALUES(@id, @key, @iv)", connection);

            cmd.Parameters.Add("@id", MySqlDbType.Guid).Value = encyrptionCredentials.Id;
            cmd.Parameters.Add("@key", MySqlDbType.VarChar).Value = encyrptionCredentials.Key;
            cmd.Parameters.Add("@iv", MySqlDbType.VarChar).Value = encyrptionCredentials.Iv;

            var result = await cmd.ExecuteNonQueryAsync();
            if(result == 0)
                throw new Exception("Query excecuted but no change was made!");
        }
        public async Task<EncyrptionCredentialsDto?> GetKeyAndIv(Guid id)
        {
            await using MySqlConnection connection = _mySQLConnectionFactory.CreateUserInfoConnection();
            await connection.OpenAsync();

            await using MySqlCommand cmd = new(@"SELECT id, aes_key, iv 
                                                 FROM keyandiv 
                                                 WHERE id = @id LIMIT 1",connection);

            cmd.Parameters.Add("@id", MySqlDbType.VarChar).Value = id;

            var reader = await cmd.ExecuteReaderAsync();
            while(await reader.ReadAsync())
            {
                return new EncyrptionCredentialsDto() {
                  Id = reader.GetGuid("id"),
                  Key = reader.GetString("aes_key"),
                  Iv = reader.GetString("iv")
                };
            }

            return null;
        }

        public async Task DeleteTokenById(string refreshToken)
        {
            await using MySqlConnection connection = _mySQLConnectionFactory.CreateUserInfoConnection();
            await connection.OpenAsync();
            await using MySqlCommand cmd = new(@"DELETE FROM refreshTokens 
                                                 WHERE token = @token", connection);

            cmd.Parameters.Add(@"token", MySqlDbType.VarChar).Value = refreshToken;
            var result = await cmd.ExecuteNonQueryAsync();

            if (result == 0)
                throw new Exception("Query Excecuted but no change was made");
        }

    }
}
