using Microsoft.AspNetCore.Identity;
using DiscussedDto.User;
using DiscussedApi.Models.UserInfo;

namespace DiscussedApi.Processing.UserPocessing
{
    public interface IUserProcessing
    {
        Task<IdentityResult> ChangePassword(RecoverUserDto recoverUser, User? user);
        Task<bool> UserAlreadyExists(string email);
    }
}
