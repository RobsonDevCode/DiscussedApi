using Discusseddto.Profile;

namespace DiscussedApi.Processing.Profile
{
    public interface IProfileProcessing
    {
        Task FollowUser(ProfileDto profile, CancellationToken ctx);
        Task UnfollowUser(ProfileDto profileDto, CancellationToken ctx);

    }
}
