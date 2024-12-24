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
            if (register == null)
                return BadRequest("Request body is empty");

            if (string.IsNullOrEmpty(register.Password))
                return BadRequest(new { error = new ErrorResponse("Password is cant be empty") });

            if (string.IsNullOrEmpty(register.UserName) || string.IsNullOrEmpty(register.EmailAddress))
                return BadRequest(new { error = new ErrorResponse("Invalid Username or Email") });

            var credentials = await _encryptor.DecryptCredentials(register.EmailAddress, register.Password, register.KeyId);

            if (await _userProcessing.UserAlreadyExists(credentials.UsernameOrEmail, register.UserName))
                return BadRequest(new { error = new ErrorResponse("Account connected to this email already exists") });

            if (string.IsNullOrEmpty(credentials.Password))
                return StatusCode(500);

            var user = new User
            {
                UserName = register.UserName,
                Email = credentials.UsernameOrEmail,
            };

            var createdUser = await _userManager.CreateAsync(user, credentials.Password);

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
                return ValidationProblem(failedValidation);

            var credentials = await _encryptor.DecryptCredentials(loginDto.UsernameOrEmail, loginDto.Password, loginDto.KeyId);

            User? user = new User();
            if (credentials.UsernameOrEmail.Contains('@'))
                user = await _userManager.Users.FirstOrDefaultAsync(x => x.Email.ToLower() == credentials.UsernameOrEmail.ToLower());
            else
                user = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == credentials.UsernameOrEmail);

            if (user == null)
                return Unauthorized($"{credentials.UsernameOrEmail} Is Not A Valid Username!");

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
        [HttpPost("recover/account")]
        public async Task<IActionResult> RecoverAccount([FromBody] RecoverUserDto recoverUser)
        {
            if (string.IsNullOrWhiteSpace(recoverUser.Email))
                return BadRequest("Attempted to recover account failed Email is incorrect");

            var unecryptedEmail = await _encryptor.DecryptStringAsync(recoverUser.Email, recoverUser.KeyId);

            var user = await _userManager.Users.FirstOrDefaultAsync(x => string.Equals(x.Email.ToLower() ,unecryptedEmail.ToLower()));

            if (user == null)
                return Unauthorized($"No account with email {recoverUser.Email} exists please create account Or check your email is correct!");

            _emailProcessing.SendRecoveryEmail(unecryptedEmail);

            return Ok();
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
                return Unauthorized();

            if (!await _authDataAccess.IsConfirmationCodeCorrect(confirmationEmailDto.ConfirmationCode))
                return Ok(new EmailConfirmationApiResponse
                {
                    Success = false,
                    Message = "Incorrect Confirmation Code Given."
                });

            user.EmailConfirmed = true;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                _logger.Error(result.Errors);
                return StatusCode(500, result.Errors);
            }

            return Ok(new EmailConfirmationApiResponse
            {
                Success = true,
                Message = "User Email Confirmed"
            });
        }


    }
}
