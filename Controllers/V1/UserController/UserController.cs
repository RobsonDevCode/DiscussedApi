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
        private NLog.ILogger _logger = LogManager.GetCurrentClassLogger();
        public UserController(UserManager<User> userManager, ITokenService tokenService, SignInManager<User> signInManager, IEmailSender emailSender, IUserProcessing userProcessing)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _userProcessing = userProcessing;
        }

        #if DEBUG
        [HttpGet("TestLogin")]
        public async Task<IActionResult> AutoLogin()
        {
            try
            {
                var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Email.ToLower() == "robsonsTester@gmail.com");

                return Ok(new NewUserDto
                {
                    UserName = user.UserName,
                    Email = user.Email,
                    Token = _tokenService.GeneratedToken(user)
                });
            }
            catch (Exception ex) {
            return StatusCode(500, ex.Message);
            }

        }
#endif

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto register)
        {
           
           if (register == null) return BadRequest("Request body sent was null");

           var user = new User
           {
               UserName = register.UserName,
               Email = register.EmailAddress
           };

           if (string.IsNullOrEmpty(register.Password) || string.IsNullOrEmpty(user.UserName) || string.IsNullOrEmpty(user.Email))
               return BadRequest("Invalid Password Or Username/Email");

            try 
            { 
                var createcUser = await _userManager.CreateAsync(user, register.Password);

                if (createcUser.Succeeded)
                {
                    var roleResult = await _userManager.AddToRoleAsync(user, "User");

                    if (roleResult.Succeeded)
                    {
                        return Ok(
                            new NewUserDto
                            {
                                UserName = user.UserName,
                                Email = user.Email,
                                Token = _tokenService.GeneratedToken(user)
                            });

                    }
                    else
                    {
                        _logger.Error(roleResult.Errors);
                        return StatusCode(500, roleResult.Errors);
                    }
                }
                else
                {
                    return BadRequest(createcUser.Errors);
                }
            }

            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                return StatusCode(500, ex);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            if (loginDto == null) return BadRequest("Login Credentials are null");

            User? user = new User();
            try
            {
                if (loginDto.UserNameOrEmail.Contains('@'))
                {
                    user = await _userManager.Users.FirstOrDefaultAsync(x => x.Email.ToLower() == loginDto.UserNameOrEmail.ToLower());
                }
                else
                {
                    user = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == loginDto.UserNameOrEmail);
                }

                if (user == null || string.IsNullOrWhiteSpace(user.UserName) || string.IsNullOrWhiteSpace(user.Email))
                {
                    return Unauthorized($"{loginDto.UserNameOrEmail} Is Not A Valid UserName!");
                }

                var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

                if (!result.Succeeded) return Unauthorized("Username/Password is incorrect or not foung");

                return Ok(new NewUserDto
                {
                    UserName = user.UserName,
                    Email = user.Email,
                    Token = _tokenService.GeneratedToken(user)
                });
            }

            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                return StatusCode(500, ex.Message);
            }
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
            try
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

                return Ok(new RecoverUserDto
                {
                    Email = recoverUser.Email
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        [Authorize]
        [HttpPost("mail/confirmation")]
        public async Task<IActionResult> EmailConfirmation([FromBody, EmailAddress] string email)
        {
            try
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
            catch(Exception ex)
            {
                _logger.Error(ex, ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        
    }
}
