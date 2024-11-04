using Discusseddto.Profile;

namespace DiscussedApi.Processing.Profile
{
    public interface IProfileProcessing
    {
        Task FollowUser(ProfileDto profile);
        Task UnfollowUser(ProfileDto profileDto);

    }
}
