using DiscussedApi.Configuration;
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

        public async Task FollowUser(ProfileDto profile, CancellationToken ctx)
        {
            if (!await DoesUserExist(profile, ctx)) throw new Exception("User deleted their account or cannot be followed!");

            await _profileDateAccess.FollowUser(profile, ctx);
        }

        public async Task UnfollowUser(ProfileDto profile, CancellationToken ctx)
        {
            if (!await DoesUserExist(profile, ctx)) throw new Exception("User deleted their account or cannot be followed!");

            await _profileDateAccess.UnFollowUser(profile, ctx);
        }

        public async Task<bool> DoesUserExist(ProfileDto profile, CancellationToken ctx)
        {
            if (profile == null) throw new ArgumentNullException($"{nameof(profile)} is cannot be null when checking if user Exists");

            bool exists = (await _userManager.Users.CountAsync(x => x.Id == profile.SelectedUser.ToString(), cancellationToken: ctx) == 0) ? false : true;

            return exists;
        }
    }
}
