using DiscussedApi.Models.UserInfo;
using DiscussedApi.Reopisitory.Profiles;
using Discusseddto.Profile;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace DiscussedApi.Processing.Profile
{
    public class ProfileProcessing : IProfileProcessing
    {
        private readonly IProfileDataAccess _profileDateAccess;
        private NLog.ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly UserManager<User> _userManager;

        public ProfileProcessing(IProfileDataAccess profileDataAccess, UserManager<User> userManager)
        {
             _profileDateAccess = profileDataAccess;
            _userManager = userManager;
        }

        public async Task FollowUser(ProfileDto profile)
        {
            try
            {
                if (!await DoesUserExist(profile)) throw new Exception("User deleted their account or cannot be followed!");

                await _profileDateAccess.FollowUser(profile); 
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                throw; 
            }
        }

        public async Task UnfollowUser(ProfileDto profile)
        {
            try
            {
                if (!await DoesUserExist(profile)) throw new Exception("User deleted their account or cannot be followed!");

                await _profileDateAccess.UnFollowUser(profile);
            }
            catch(Exception ex)
            {
                _logger.Error(ex);
                throw;
            }
            throw new NotImplementedException();
        }

        public async Task<bool> DoesUserExist(ProfileDto profile)
        {
            try
            {
                if (profile == null) throw new ArgumentNullException($"{nameof(profile)} is cannot be null when checking if user Exists");

                bool exists = (await _userManager.Users.CountAsync(x => x.Id.Equals(profile.SelectedUser)) == 0) ? false : true;

                return exists;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
