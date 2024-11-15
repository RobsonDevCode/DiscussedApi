using DiscussedApi.Models.Profiles;
using Discusseddto.Profile;
using System.Collections.Immutable;

namespace DiscussedApi.Reopisitory.Profiles
{
    public interface IProfileDataAccess
    {
        Task<List<Guid?>> GetUserFollowing(Guid userGuid);

        Task FollowUser(ProfileDto profile);

        Task UnFollowUser(ProfileDto profile);
    }
}
