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
            if (user == null) return IdentityResult.Failed(new IdentityError { Code = "ArgumentNull", Description = "Attempted Password change request denied user given was null" });

            user.PasswordHash = recoverUser.NewPassword;

            var removePassword = await _userManager.RemovePasswordAsync(user);

            if (!removePassword.Succeeded)
                return removePassword;

            return await _userManager.AddPasswordAsync(user, recoverUser.NewPassword);

        }

        public async Task<bool> UserAlreadyExists(string email) =>
           await _userManager.FindByEmailAsync(email) != null;
    }
}
