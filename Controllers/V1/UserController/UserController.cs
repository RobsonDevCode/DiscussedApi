using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using DiscussedApi.Configuration;
using DiscussedApi.Processing.UserPocessing;
using DiscussedApi.Services.Email;
using DiscussedApi.Services.Tokens;
using DiscussedDto.User;
using System.ComponentModel.DataAnnotations;
using NLog;
using DiscussedApi.Models.UserInfo;
using DiscussedApi.Models.Auth;
using System.Security.Authentication;
using DiscussedApi.Authenctication;
using DiscussedApi.Models.Error;
using DiscussedApi.Extentions;

namespace DiscussedApi.Controllers.V1.UserController
{
    [ApiController]
    [Route("V1/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly ITokenService _tokenService;
        private readonly SignInManager<User> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly IUserProcessing _userProcessing;
        private readonly IEncryptor _encryptor;
        private NLog.ILogger _logger = LogManager.GetCurrentClassLogger();
        public UserController(UserManager<User> userManager, ITokenService tokenService, SignInManager<User> signInManager,
            IEmailSender emailSender, IUserProcessing userProcessing, IEncryptor encryptor)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _userProcessing = userProcessing;
            _encryptor = encryptor;
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

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto register)
        {
            if (register == null) 
                return BadRequest("Request body sent was null");

            var user = new User
            {
                UserName = register.UserName,
                Email = register.EmailAddress
            };

            if (string.IsNullOrEmpty(register.Password))
                return BadRequest(new { error = new ErrorResponse("Password is cant be empty") });

            if (string.IsNullOrEmpty(user.UserName) || string.IsNullOrEmpty(user.Email))
                return BadRequest(new { error = new ErrorResponse("Invalid Username or Email") });

            if (await _userProcessing.UserAlreadyExists(user.Email))
                return BadRequest(new { error = new ErrorResponse("Account connected to this email already exists") });

            var password = await _encryptor.DecryptPassword(register.Password, register.KeyId);

            if (string.IsNullOrEmpty(password))
                return StatusCode(500);
            
            var createdUser = await _userManager.CreateAsync(user, password);
            if (!createdUser.Succeeded)
            {
                _logger.Error(createdUser.Errors);
                return BadRequest(new
                {
                    errors = createdUser.Errors
                });
            }

            var roleResult = await _userManager.AddToRoleAsync(user, "User");

            if (!roleResult.Succeeded)
            {
                _logger.Error(roleResult.Errors);
                return StatusCode(500, new
                {
                    errors = roleResult.Errors
                });
            }
            //set jwt first as we dont want decoupled refresh tokens
            var tokens = await _tokenService.GenerateAndSetJwtAndRefreshToken(user, Response);

            return Ok();
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            if (loginDto == null)
                return BadRequest("Login Credentials are null");

            User? user = new User();
            if (loginDto.UserNameOrEmail.Contains('@'))
            {
                user = await _userManager.Users.FirstOrDefaultAsync(x => x.Email.ToLower() == loginDto.UserNameOrEmail.ToLower());
            }
            else
            {
                user = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == loginDto.UserNameOrEmail);
            }

            if (user == null || string.IsNullOrWhiteSpace(user.UserName) || string.IsNullOrWhiteSpace(user.Email))
                return Unauthorized($"{loginDto.UserNameOrEmail} Is Not A Valid UserName!");

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

            if (!result.Succeeded)
                return Unauthorized("Username/Password is incorrect or not found");

            var tokens = await _tokenService.GenerateAndSetJwtAndRefreshToken(user, Response);

            if (string.IsNullOrWhiteSpace(tokens.Jwt))
                throw new BuildTokenException($"Error when building JWT when presented user {user.UserName}");

            if (tokens.RefreshToken == null)
                throw new BuildTokenException($"Error when building Refresh Token when presented user {user.UserName}");

            if (string.IsNullOrWhiteSpace(tokens.RefreshToken.Token))
                throw new BuildTokenException($"Error when building Refresh Token when presented user {user.UserName}");

            return Ok(new
            {
                user_name = user.UserName,
            });
        }



        [Authorize]
        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            var user = new User();

            return Ok(user);
        }

        [HttpPost("recover/account")]
        public async Task<IActionResult> RecoverAccount([FromBody] RecoverUserDto recoverUser)
        {
            if (string.IsNullOrWhiteSpace(recoverUser.NewPassword) || string.IsNullOrWhiteSpace(recoverUser.Email))
                return BadRequest("Attempted to recover account failed Email or Password is incorrect");

            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Email.ToLower() == recoverUser.Email.ToLower());

            if (user == null)
            {
                return BadRequest($"No account with email {recoverUser.Email} exists please create account Or check your email is correct!");
            }

            var result = await _userProcessing.ChangePassword(recoverUser, user);

            if (!result.Succeeded) return StatusCode(500, result.Errors);

            return Ok(new
            {
                success = "password changed"
            });
        }

        [HttpPost("mail/confirmation")]
        public async Task<IActionResult> EmailConfirmation([FromBody, EmailAddress] string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return BadRequest("Email given was null");

            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Email.ToLower() == email.ToLower());

            if (user == null)
            {
                _logger.Warn($"Failed to send email confirmation for {email}");
                return NoContent();
            }

            user.EmailConfirmed = true;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                _logger.Error(result.Errors);
                return StatusCode(500, result.Errors);
            }

            return Ok();
        }


    }
}
