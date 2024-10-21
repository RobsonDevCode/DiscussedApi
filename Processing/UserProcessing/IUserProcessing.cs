using Microsoft.AspNetCore.Identity;
using DiscussedApi.Models;
using DiscussedDto.User;

namespace DiscussedApi.Processing.UserPocessing
{
    public interface IUserProcessing
    {
         Task<IdentityResult> ChangePassword(RecoverUserDto recoverUser, User? user);
    }
}
