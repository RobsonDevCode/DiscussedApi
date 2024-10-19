using AesEncryptor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PFMSApi.Configuration;
using PFMSApi.Models;
using PFMSApi.Processing.UserPocessing;
using PFMSApi.Services.Email;
using PFMSApi.Services.Tokens;
using PFMSDdto.Email;
using PFMSDdto.User;
using RestSharp;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PFMSApi.Controllers.V1.UserController
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
        public UserController(UserManager<User> userManager, ITokenService tokenService, SignInManager<User> signInManager, IEmailSender emailSender, IUserProcessing userProcessing)
        {
           _userManager = userManager;
           _tokenService = tokenService;
           _signInManager = signInManager;
           _emailSender = emailSender;
           _userProcessing = userProcessing; 
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto register)
        {
          try
          {
                if (register == null) return BadRequest("Request body sent was null");

                var user = new User
                {
                    UserName = register.UserName,
                    Email = register.EmailAddress
                };

                if (string.IsNullOrEmpty(register.Password) || string.IsNullOrEmpty(user.UserName) || string.IsNullOrEmpty(user.Email)) 
                    return BadRequest("Invalid Password Or Username/Email");

                var createcUser = await _userManager.CreateAsync(user, register.Password);

                if(createcUser.Succeeded)
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
                        return StatusCode(500, roleResult.Errors);
                    }
                }
                else
                {
                    return BadRequest(createcUser.Errors);
                }
          }

          catch(Exception ex)
          {
            Console.WriteLine(ex);
            return StatusCode(500, ex);
          }
        }

        [HttpPost("Login")]
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

            catch(Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(500, ex.Message);
            }
        }

        [Authorize]
        [HttpGet("LogOut")]

        public async Task<IActionResult> Logout()
        {
            var user = new User();

            return Ok(user);
        }

        [HttpPost("RecoverAccount")]
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
                Console.WriteLine(ex);
                return StatusCode(500, ex.Message);
            }
        }

        [Authorize]
        [HttpPost("EmailConfirmation")]
        public async Task<IActionResult> EmailConfirmation([FromBody, EmailAddress] string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return BadRequest("Email given was null");

            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Email.ToLower() == email.ToLower());

            if (user == null)
            {
                return NoContent();
            }

            user.EmailConfirmed = true;
            var result = await _userManager.UpdateAsync(user);

            if(!result.Succeeded) return StatusCode(500,result.Errors);

            return Ok();


        }

        
    }
}
