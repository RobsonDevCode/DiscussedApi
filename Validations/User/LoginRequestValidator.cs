using DiscussedDto.User;
using FluentValidation;

namespace DiscussedApi.Validations.User
{
    public class LoginRequestValidator : AbstractValidator<LoginDto>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.UsernameOrEmail).NotEmpty().WithMessage("Username or Email cannot be empty");
            RuleFor(x => x.Password).NotEmpty().WithMessage("Password cant be empty");
            RuleFor(x => x.KeyId).NotEmpty().WithMessage("Key id can't be empty");
        }
    }
}
