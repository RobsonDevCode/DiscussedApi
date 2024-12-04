using DiscussedApi.Abstraction;
using DiscussedApi.Models.Profiles;
using DiscussedApi.Models.UserInfo;
using Discusseddto.Profile;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using System.Collections.Immutable;
using System.Data;

namespace DiscussedApi.Reopisitory.Profiles
{
    public class ProfileDataAccess : IProfileDataAccess
    {
        private readonly IMySqlConnectionFactory _mySqlConnectionFactory;
        public ProfileDataAccess(IMySqlConnectionFactory mySqlConnectionFactory)
        {
            _mySqlConnectionFactory = mySqlConnectionFactory;
        }

        public async Task<List<Guid?>> GetUserFollowing(Guid userId, CancellationToken ctx)
        {
            await using MySqlConnection connection = _mySqlConnectionFactory.CreateUserInfoConnection();

            await connection.OpenAsync();
            string sql = @"SELECT UserFollowing FROM following WHERE UserGuid = @userId;";

            await using MySqlCommand cmd = new(sql, connection);
            cmd.Parameters.Add("@userId", MySqlDbType.Guid).Value = userId;

            using MySqlDataReader reader = await cmd.ExecuteReaderAsync();
            List<Guid?> following = new List<Guid?>();

            while (await reader.ReadAsync())
            {
                following.Add(reader.GetGuid("UserFollowing"));
            }

            return following;
        }


        public async Task FollowUser(ProfileDto profile, CancellationToken ctx)
        {
            await using MySqlConnection connection = _mySqlConnectionFactory.CreateUserInfoConnection();
            await connection.OpenAsync(ctx);

            await using MySqlCommand cmd = new(@"INSERT INTO following (UserGuid, UserName, UserFollowing, IsFollowing)
                                                VALUES(@userGuid, @userName, @userSelected, 1)", connection);

            cmd.Parameters.Add("@userGuid", MySqlDbType.Guid).Value = profile.UserGuid;
            cmd.Parameters.Add("@userName", MySqlDbType.VarChar).Value = profile.UserName;
            cmd.Parameters.Add("@userSelected", MySqlDbType.Guid).Value = profile.SelectedUser;

            var result = await cmd.ExecuteNonQueryAsync(ctx);

            if (result == 0)
                throw new Exception("Data query was executed but no change was made");
        }


        public async Task UnFollowUser(ProfileDto profile, CancellationToken ctx)
        {
            await using MySqlConnection connection = _mySqlConnectionFactory.CreateUserInfoConnection();
            await connection.OpenAsync(ctx);

            await using MySqlCommand cmd = new(@"DELETE FROM following WHERE UserGuid = @userGuid AND UserFollowing = @selectedUser", connection);
            cmd.Parameters.Add("@userGuid", MySqlDbType.Guid).Value = profile.UserGuid;
            cmd.Parameters.Add("@selectedUser", MySqlDbType.Guid).Value = profile.SelectedUser;

            var result = await cmd.ExecuteNonQueryAsync(ctx);
            if (result == 0)
                throw new Exception("Data query was executed but no change was made");
        }


    }
}
