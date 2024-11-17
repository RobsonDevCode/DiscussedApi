using Discusseddto.CommentDtos;
using FluentValidation;

namespace DiscussedApi.Validations.Comment
{
    public class EditCommentRequestValidation : AbstractValidator<UpdateCommentDto>
    {
        public EditCommentRequestValidation()
        {
            RuleFor(x => x.UserId).NotNull().WithMessage("User Id cannot be null when editing comment!");
            RuleFor(x => x.UserId).NotEmpty().WithMessage("User Id cannot be empty when editing comment!");
            RuleFor(x => x.CommentId).NotNull().WithMessage("Comment Id cannot be null when editing comment!");
            RuleFor(x => x.CommentId).NotNull().WithMessage("Comment Id cannot be empty when editing comment!");
            RuleFor(x => x.Content).Length(1, 250).WithMessage("Comment has to be atleast 1 character long and a Max of 250!");

        }
    }
}
