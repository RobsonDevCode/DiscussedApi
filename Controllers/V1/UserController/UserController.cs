using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DiscussedApi.Common.Validations;

using Newtonsoft.Json;
using DiscussedApi.Configuration;
using DiscussedApi.Processing.UserPocessing;
using DiscussedApi.Services.Email;
using DiscussedApi.Services.Tokens;
using DiscussedDto.User;
using NLog;
using DiscussedApi.Models.UserInfo;
using DiscussedApi.Models.Auth;
using System.Security.Authentication;
using DiscussedApi.Authenctication;
using DiscussedApi.Extentions;
using DiscussedApi.Reopisitory.Auth;
using Discusseddto.Email;
using DiscussedApi.Models.ApiResponses.Email;
using DiscussedApi.Models.ApiResponses.Error;
using static DiscussedApi.Models.EmailTypeToGenertate;
using DiscussedApi.Processing;
using FluentValidation;
using DiscussedApi.Models.ApiResponses;

namespace DiscussedApi.Controllers.V1.UserController
{
    [ApiController]
    [Route("V1/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly ITokenService _tokenService;
        private readonly SignInManager<User> _signInManager;
        private readonly IUserProcessing _userProcessing;
        private readonly IAuthDataAccess _authDataAccess;
        private readonly IEncryptor _encryptor;
        private readonly IEmailProcessing _emailProcessing;
        private NLog.ILogger _logger = LogManager.GetCurrentClassLogger();
        public UserController(UserManager<User> userManager, ITokenService tokenService, SignInManager<User> signInManager,
            IUserProcessing userProcessing, IAuthDataAccess authDataAccess,
            IEncryptor encryptor, IEmailProcessing emailProcessing)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _signInManager = signInManager;
            _userProcessing = userProcessing;
            _authDataAccess = authDataAccess;
            _encryptor = encryptor;
            _emailProcessing = emailProcessing;
        }

#if DEBUG
        [HttpGet("TestLogin")]
        public async Task<IActionResult> AutoLogin()
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Email.ToLower() == "robsonsTester@gmail.com");
            return Ok(new UserDto
            {
                UserName = user.UserName,
                Email = user.Email,
                Token = _tokenService.GeneratedToken(user)
            });

        }
#endif

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto register)
        {
            if (string.IsNullOrEmpty(register.Password))
                throw new ValidationException("Password is cant be empty");

            if (string.IsNullOrEmpty(register.UserName) || string.IsNullOrEmpty(register.EmailAddress))
                throw new ValidationException("Invalid Username or Email");

            var credentials = await _encryptor.DecryptCredentials(register.EmailAddress, register.Password, register.KeyId);

            if (await _userProcessing.UserAlreadyExists(credentials.UsernameOrEmail, register.UserName))
                throw new ValidationException("Account connected to this email already exists");

            var user = new User
            {
                UserName = register.UserName,
                Email = credentials.UsernameOrEmail,
            };

            var createdUser = await _userManager.CreateAsync(user, credentials.Password);

            if (!createdUser.Succeeded)
            {
                var errorDescription = createdUser.Errors
                                        ?.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Description))
                                        ?.Description ?? "Unexpected error while creating user. Please try again or contact support.";

                throw new InvalidOperationException(errorDescription);
            }

            var roleResult = await _userManager.AddToRoleAsync(user, "User");

            if (!roleResult.Succeeded)
            {
                var errorDescription = roleResult.Errors
                                         ?.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Description))
                                         ?.Description ?? "Unexpected error while creating user. Please try again or contact support.";

                throw new InvalidOperationException(errorDescription);
            }

            //fire and forget email
            _emailProcessing.SendConfirmationEmail(credentials.UsernameOrEmail);

            return Ok();
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto, [FromServices] IValidator<LoginDto> validator)
        {
            var failedValidation = await Validator<LoginDto>.TryValidateRequest(loginDto, validator);

            if (failedValidation != null)
                throw new ValidationException(failedValidation?.FirstOrDefault().Value?.AttemptedValue); //get first validation error

            var credentials = await _encryptor.DecryptCredentials(loginDto.UsernameOrEmail, loginDto.Password, loginDto.KeyId);

            User? user = new User();
            if (credentials.UsernameOrEmail.Contains('@'))
                user = await _userManager.Users.FirstOrDefaultAsync(x => x.Email.ToLower() == credentials.UsernameOrEmail.ToLower());
            else
                user = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == credentials.UsernameOrEmail);

            if (user == null)
                throw new ValidationException($"{credentials.UsernameOrEmail} Is Not A Valid Username!");

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

            if (!result.Succeeded)
                throw new ValidationException("Username/Password is incorrect or not found");

            await _tokenService.GenerateAndSetJwtAndRefreshToken(user, Response);

            return Ok(new
            {
                user_name = user.UserName,
            });
        }


        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize]
        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            var user = new User();

            return Ok(user);
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] RecoverUserDto recoverUser)
        {

            var credentials = await _encryptor.DecryptCredentials(recoverUser.Email, recoverUser.NewPassword, recoverUser.KeyId);

            var user = await _userManager.FindByEmailAsync(recoverUser.Email.ToLower());

            //return ok as we dont want to give away an infomation regarding the email 
            if (user == null)
            {
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = new { password_changed = false }
                });
            }

            if (await _userManager.CheckPasswordAsync(user, credentials.Password))
                throw new BadHttpRequestException("Password has already been used in the last 3 months please use a different password");

            string? passwordToken = Request.Cookies[$"reset_password_token_{user.UserName}"];

            if (string.IsNullOrWhiteSpace(passwordToken))
            {
                _logger.Error($"Invalid password token: {passwordToken}");
                throw new InvalidOperationException("Invalid or expired reset token");
            }

            if (!await _tokenService.IsValidPasswordResetToken(passwordToken))
            {
                _logger.Error($"Invalid password token: {passwordToken}");
                return Ok(new ApiResponse<object>
                {
                    Success = false,
                    Data = new { password_changed = false }
                });
            }

            var result = await _userManager.ResetPasswordAsync(user, passwordToken, recoverUser.NewPassword);

            if (!result.Succeeded)
            {
                var errorDescription = result.Errors
                                        ?.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Description))
                                        ?.Description ?? "Unexpected error while resetting password. Please try again or contact support.";
                throw new InvalidOperationException(errorDescription);
            }

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = new { passwordChanged = true }
            });
        }


        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost("email/confirmation")]
        public async Task<IActionResult> EmailConfirmation([FromBody] ConfirmationCodeDto confirmationEmailDto)
        {
            var unecryptedEmail = await _encryptor.DecryptStringAsync(confirmationEmailDto.Email, confirmationEmailDto.Key);

            var user = await _userManager.FindByEmailAsync(unecryptedEmail);
            if (user == null)
                throw new Exception("No user linked to this code");

            if (!await _authDataAccess.IsConfirmationCodeCorrect(confirmationEmailDto.ConfirmationCode))
                return Ok(new ApiResponse<object>
                {
                    Success = false,
                    Data = new { message = "Incorrect code given!"}
                });

            user.EmailConfirmed = true;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                _logger.Error(result.Errors);
                var errorDescription = result.Errors
                                          ?.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Description))
                                          ?.Description ?? "Unexpected error while resetting password. Please try again or contact support.";
                throw new InvalidOperationException(errorDescription);
            }

            return Ok(new ApiResponse<object>
            {
                Success = true
            });
        }


    }
}
