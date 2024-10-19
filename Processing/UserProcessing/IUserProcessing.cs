using Microsoft.AspNetCore.Identity;
using PFMSApi.Models;
using PFMSDdto.User;

namespace PFMSApi.Processing.UserPocessing
{
    public interface IUserProcessing
    {
         Task<IdentityResult> ChangePassword(RecoverUserDto recoverUser, User? user);
    }
}
