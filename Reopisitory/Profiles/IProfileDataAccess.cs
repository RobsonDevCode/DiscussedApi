using DiscussedApi.Models.Profiles;
using Discusseddto.Profile;
using System.Collections.Immutable;

namespace DiscussedApi.Reopisitory.Profiles
{
    public interface IProfileDataAccess
    {
        Task<List<Guid?>> GetUserFollowing(Guid userGuid, CancellationToken ctx);

        Task FollowUser(ProfileDto profile, CancellationToken ctx);

        Task UnFollowUser(ProfileDto profile, CancellationToken ctx);
    }
}
