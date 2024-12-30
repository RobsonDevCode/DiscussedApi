using DiscussedApi.Abstraction;
using DiscussedApi.Configuration;
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

        public async Task<RefreshToken?> GetRefreshTokenByIdAsync(string tokenSent)
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

        public async Task<ResetPasswordToken?> GetPasswordTokenAsync(string tokenSent)
        {
            await using MySqlConnection connection = _mySQLConnectionFactory.CreateUserInfoConnection();
            await connection.OpenAsync();
            await using MySqlCommand cmd = new MySqlCommand(@"SELECT email,
                                                              token, 
                                                              expiresOnUtc FROM passwordchangetokens 
                                                              WHERE token = @token LIMIT 1", connection);

            cmd.Parameters.Add("@token", MySqlDbType.VarChar).Value = tokenSent;

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new ResetPasswordToken()
                {
                    Email = reader.GetString("email"),
                    Token = reader.GetString("token"),
                    ExpiresOnUtc = reader.GetDateTime("expiresOnUtc")
                };
            }

            return null;
        }
        public async Task<(bool IsValid, string? ValidUserEmail)> ValidPasswordRefreshToken(string token)
        {
            if(string.IsNullOrWhiteSpace(token))
                throw new ArgumentNullException("failed validating password token, token is null");

            await using MySqlConnection connection = _mySQLConnectionFactory.CreateUserInfoConnection();
            await connection.OpenAsync();
            await using MySqlCommand cmd = new MySqlCommand(@"SELECT email, 
                                                                     token, 
                                                                     expiresOnUtc 
                                                              FROM passwordchangetokens WHERE token = @token
                                                              ORDER BY expiresOnUtc DESC LIMIT 1", connection); 

            cmd.Parameters.Add("@token", MySqlDbType.VarChar).Value = token;

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                ResetPasswordToken resetPasswordToken = new ResetPasswordToken()
                {
                    Email = reader.GetString("email"),
                    Token = reader.GetString("token"),
                    ExpiresOnUtc = reader.GetDateTime("expiresOnUtc")
                };

                if(resetPasswordToken.ExpiresOnUtc <= DateTime.UtcNow)
                    return (false, null);

                return (true, resetPasswordToken.Email);
            }

            return (false, null);
        }

        public async Task StoreRefreshTokenAsync(RefreshToken token)
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

        public async Task StoreKeyAndIvAsync(EncyrptionCredentialsDto encyrptionCredentials)
        {
            await using MySqlConnection connection = _mySQLConnectionFactory.CreateUserInfoConnection();
            await connection.OpenAsync();

            await using MySqlCommand cmd = new(@"INSERT INTO keyandiv(id, aes_key, iv, expire_date) 
                                                 VALUES(@id, @key, @iv, NOW())", connection);

            cmd.Parameters.Add("@id", MySqlDbType.Guid).Value = encyrptionCredentials.Id;
            cmd.Parameters.Add("@key", MySqlDbType.VarChar).Value = encyrptionCredentials.Key;
            cmd.Parameters.Add("@iv", MySqlDbType.VarChar).Value = encyrptionCredentials.Iv;

            var result = await cmd.ExecuteNonQueryAsync();
            if (result == 0)
                throw new Exception("Query excecuted but no change was made!");
        }

        public async Task<EncyrptionCredentialsDto?> GetKeyAndIvAsync(Guid id)
        {
            try
            {
                await using MySqlConnection connection = _mySQLConnectionFactory.CreateUserInfoConnection();
                await connection.OpenAsync();

                await using MySqlCommand cmd = new(@"SELECT id, aes_key, iv, expire_date
                                                 FROM keyandiv 
                                                 WHERE id = @id LIMIT 1", connection);

                cmd.Parameters.Add("@id", MySqlDbType.VarChar).Value = id;

                var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    return new EncyrptionCredentialsDto()
                    {
                        Id = reader.GetGuid("id"),
                        Key = reader.GetString("aes_key"),
                        Iv = reader.GetString("iv"),
                        ExpireTime = reader.GetDateTime("expire_date")
                    };
                }


                return null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        public async Task DeleteKeyAndIvByIdAsync(Guid id)
        {
            await using MySqlConnection connection = _mySQLConnectionFactory.CreateUserInfoConnection();
            await connection.OpenAsync();

            await using MySqlCommand cmd = new(@"DELETE FROM keyandiv WHERE id = @id", connection);
            cmd.Parameters.Add("@id", MySqlDbType.Guid).Value = id;

            var result = await cmd.ExecuteNonQueryAsync();

            if (result == 0)
                throw new Exception("Query excecuted but no change was made!");
        }

        public async Task DeleteTokenByIdAsync(string refreshToken)
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

        public async Task StorePassordResetToken(string? email, string token)
        {
            await using MySqlConnection connection = _mySQLConnectionFactory.CreateUserInfoConnection();
            await connection.OpenAsync();
            await using MySqlCommand cmd = new(@"INSERT INTO passwordchangetokens (email, token, expiresOnUtc)
                                                 VALUES (@email, @token, @expiresAt)", connection);

            cmd.Parameters.Add("@email", MySqlDbType.VarChar).Value = email;
            cmd.Parameters.Add("@token", MySqlDbType.VarChar).Value = token;
            cmd.Parameters.Add("@expiresAt", MySqlDbType.DateTime).Value = DateTime.UtcNow.AddMinutes(Settings.Encryption.PasswordResetExpireTime);

            var result = await cmd.ExecuteNonQueryAsync();

            if (result == 0)
                throw new Exception("Query Excecuted but no change was made");

        }
        public async Task StoreEmailConfirmationCodeAsync(string email, int confirmationNum)
        {
            await using MySqlConnection connection = _mySQLConnectionFactory.CreateUserInfoConnection();
            await connection.OpenAsync();
            await using MySqlCommand cmd = new(@"INSERT INTO confirmationemail (email, confirmation_code)
                                                 VALUES(@email, @confirmation_code)", connection);

            cmd.Parameters.Add("@email", MySqlDbType.VarChar).Value = email;
            cmd.Parameters.Add("@confirmation_code", MySqlDbType.Int32).Value = confirmationNum;

            var result = await cmd.ExecuteNonQueryAsync();

            if (result == 0)
                throw new Exception("Query excecuted but no change was made!");
        }

        public async Task<bool> IsConfirmationCodeCorrect(int confirmationCode)
        {
            await using MySqlConnection connection = _mySQLConnectionFactory.CreateUserInfoConnection();
            await connection.OpenAsync();
            await using MySqlCommand cmd = new(@"SELECT COUNT(*) FROM user_info.confirmationemail 
                                                 WHERE confirmation_code = @code", connection);

            cmd.Parameters.Add("@code", MySqlDbType.Int32).Value = confirmationCode;

            ushort count = Convert.ToUInt16(await cmd.ExecuteScalarAsync() ?? 0);
            if (count > 0)
                return true;

            return false;
        }


    }
}
