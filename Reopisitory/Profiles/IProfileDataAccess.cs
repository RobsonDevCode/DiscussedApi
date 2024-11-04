using DiscussedApi.Models.Profiles;
using Discusseddto.Profile;

namespace DiscussedApi.Reopisitory.Profiles
{
    public interface IProfileDataAccess
    {
        Task<List<Following>> GetUserFollowing(string userGuid);

        Task FollowUser(ProfileDto profile);

        Task UnFollowUser(ProfileDto profile);
        Task<bool> DoesUserExistAsync(ProfileDto profile);
    }
}
