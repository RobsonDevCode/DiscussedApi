using DiscussedApi.Models.Profiles;

namespace DiscussedApi.Reopisitory.Profiles
{
    public interface IProfileDataAccess
    {
        Task<List<Following>> GetUserFollowing(string userGuid);
    }
}
