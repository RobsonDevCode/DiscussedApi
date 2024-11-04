using Discusseddto.Profile;
using FluentValidation;

namespace DiscussedApi.Validations
{
    public class PostFollowRequestValidator : AbstractValidator<ProfileDto>
    {
        public PostFollowRequestValidator() 
        {
            RuleFor(x => x.UserGuid).NotNull().WithMessage("User UserGuid can't be null or empty");
            RuleFor(x => x.UserName).NotNull().WithMessage("UserName can't be null");
            RuleFor(x => x.SelectedUser).NotNull().WithMessage("Selected UserName can't be null");

        }
    }
}
