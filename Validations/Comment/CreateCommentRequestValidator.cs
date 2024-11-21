using Discusseddto.CommentDtos;
using FluentValidation;

namespace DiscussedApi.Validations.Comment
{
    public class CreateCommentRequestValidator : AbstractValidator<NewCommentDto>
    {
        public CreateCommentRequestValidator()
        {
            RuleFor(x => x.Content).Length(1, 250).WithMessage("Comment has to be atleast 1 character long and a Max of 250");
            RuleFor(x => x.Content).NotEmpty().WithMessage("Comment content cannot be null");
            RuleFor(x => x.UserId).NotNull().WithMessage("User Id can't be null or empty");
            RuleFor(x => x.UserName).NotNull().WithMessage("User name can't be null when posting a comment");
            RuleFor(x => x.UserName).Length(4, 150).WithMessage("Username is invalid must be between 4 - 150 characters");
            RuleFor(x => x.Id).NotNull().WithMessage("Comment Id cant be null");

        }
    }
}
