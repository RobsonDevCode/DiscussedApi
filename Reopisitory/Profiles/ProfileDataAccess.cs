using DiscussedApi.Models.Profiles;

namespace DiscussedApi.Reopisitory.Profiles
{
    public class ProfileDataAccess : IProfileDataAccess
    {
        public Task<List<Following>> GetUserFollowing(string userGuid)
        {
            throw new NotImplementedException();
        }
    }
}
