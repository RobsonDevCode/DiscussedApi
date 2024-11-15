using DiscussedApi.Data.Profiles;
using DiscussedApi.Models.Profiles;
using DiscussedApi.Models.UserInfo;
using Discusseddto.Profile;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;

namespace DiscussedApi.Reopisitory.Profiles
{
    public class ProfileDataAccess : IProfileDataAccess
    {
        ProfileDBContext _profileDBContext = new();
        private readonly NLog.ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

        public async Task<List<Guid?>> GetUserFollowing(Guid userId)
        {
            try
            {
                var following = await _profileDBContext.Following
                    .Where(x => x.UserGuid == userId)
                    .Select(x => x.UserFollowing)
                    .Take(100)
                    .ToListAsync();

                return following;
            }
            catch(Exception ex)
            {
                _logger.Error(ex);
                throw;
            }
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

                await _profileDBContext.Following.AddAsync(following);
                var result = await _profileDBContext.SaveChangesAsync();

                if (result == 0) throw new Exception("Data query was executed but no change was made");
                
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
                
            _profileDBContext.Following.Remove(following);
            var result = await _profileDBContext.SaveChangesAsync();
               
            if (result == 0) 
                throw new Exception("Data query was executed but no change was made");

        }

       
    }
}
