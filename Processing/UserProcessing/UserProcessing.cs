using Microsoft.AspNetCore.Identity;
using PFMSApi.Models;
using PFMSApi.Processing.UserPocessing;
using PFMSDdto.User;

namespace PFMSApi.Processing.UserProcessing
{
    public class UserProcessing : IUserProcessing
    {
        private readonly UserManager<User> _userManager;

        public UserProcessing(UserManager<User> userManager)
        {
             _userManager = userManager;
        }


        public async Task<IdentityResult> ChangePassword(RecoverUserDto recoverUser, User? user)
        {
            if (user == null) return IdentityResult.Failed(new IdentityError {Code = "ArgumentNull", Description = "Attempted Password change request denied user given was null"} );

            user.PasswordHash = recoverUser.NewPassword;

            var removePassword = await _userManager.RemovePasswordAsync(user);

            if (!removePassword.Succeeded)
            {
                return removePassword;
            }

            var result = await _userManager.AddPasswordAsync(user, recoverUser.NewPassword);

            return result;
        }
    }
}
