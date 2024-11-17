using Discusseddto.CommentDtos;
using FluentValidation;

namespace DiscussedApi.Validations.Comment
{
    public class LikeCommentValidator : AbstractValidator<LikeCommentDto>
    {
        public LikeCommentValidator()
        {
            RuleFor(x => x.UserId).NotNull().NotEmpty().WithMessage("User Id cannot be null when liking a Comment");
            RuleFor(x => x.CommentId).NotNull().NotEmpty().WithMessage("Comment Id cannot be null when liking a Comment");
        }
    }
}
