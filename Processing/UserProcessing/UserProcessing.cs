using Microsoft.AspNetCore.Identity;
using DiscussedApi.Processing.UserPocessing;
using DiscussedDto.User;
using DiscussedApi.Models.UserInfo;

namespace DiscussedApi.Processing.UserProcessing
{
    public class UserProcessing : IUserProcessing
    {
        private readonly UserManager<User> _userManager;
        private readonly NLog.ILogger _logger = NLog.LogManager.GetCurrentClassLogger();
        public UserProcessing(UserManager<User> userManager)
        {
             _userManager = userManager;
        }


        public async Task<IdentityResult> ChangePassword(RecoverUserDto recoverUser, User? user)
        {
            if (user == null) return IdentityResult.Failed(new IdentityError {Code = "ArgumentNull", Description = "Attempted Password change request denied user given was null"} );

            user.PasswordHash = recoverUser.NewPassword;

            try
            {
                var removePassword = await _userManager.RemovePasswordAsync(user);

                if (!removePassword.Succeeded)
                {
                    return removePassword;
                }

                var result = await _userManager.AddPasswordAsync(user, recoverUser.NewPassword);

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                return IdentityResult.Failed(new IdentityError {Code = "Change Password Error", Description = ex.Message});
            }
        }
    }
}
