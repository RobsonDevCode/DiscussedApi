using DiscussedApi.Data.Profiles;
using DiscussedApi.Models.Profiles;
using DiscussedApi.Models.UserInfo;
using Discusseddto.Profile;
using Microsoft.EntityFrameworkCore;

namespace DiscussedApi.Reopisitory.Profiles
{
    public class ProfileDataAccess : IProfileDataAccess
    {
        ProfileDBContext _profileDBContext = new();
        private readonly NLog.ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

        public async Task<List<Following>> GetUserFollowing(string userId)
        {
            try
            {
                if (!Guid.TryParse(userId, out Guid userIdAsGuid)) throw new FormatException($"Error converting: {userId} to a Guid"); 

                var following = await _profileDBContext.Following.Where(x => x.UserGuid == userIdAsGuid)
                    .Take(100)
                    .ToListAsync();

                return following;
            }
            catch(Exception ex)
            {
                _logger.Error(ex);
                throw;
            }
            throw new NotImplementedException();
        }

        public async Task FollowUser(ProfileDto profile)
        {
            try
            {
                Following following = new Following
                {
                    UserName = profile.UserName,
                    UserGuid = profile.UserGuid,
                    UserFollowing = profile.SelectedUser,
                    IsFollowing = true
                };

                using (var connection = _profileDBContext)
                {
                    await connection.Following.AddAsync(following);

                    var result = await connection.SaveChangesAsync();
                    if (result == 0) throw new Exception("Data query was executed but no change was made");
                }
                
            }
            catch(Exception ex)
            {
                _logger.Error(ex, ex.Message);
                throw;
            }
        }

        public async Task UnFollowUser(ProfileDto profile)
        {
            Following following = new Following
            {
                UserName = profile.UserName,
                UserGuid = profile.UserGuid,
                UserFollowing = profile.SelectedUser,
                IsFollowing = false
            };
            using (var connection = _profileDBContext)
            {
               connection.Remove(following);

               var result = await connection.SaveChangesAsync();
               if (result == 0) throw new Exception("Data query was executed but no change was made");
            }

        }

        public Task<bool> DoesUserExistAsync(ProfileDto profile)
        {
            throw new NotImplementedException();
        }
    }
}
