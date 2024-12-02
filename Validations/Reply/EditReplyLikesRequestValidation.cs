using Discusseddto.CommentDtos.ReplyDtos;
using FluentValidation;

namespace DiscussedApi.Validations.Reply
{
    public class EditReplyLikesRequestValidation : AbstractValidator<EditReplyLikesDto>
    {
        public EditReplyLikesRequestValidation()
        {
            RuleFor(x => x.UserId).NotNull().NotEmpty().WithMessage("user_id cannot be null when liking a Comment");
            RuleFor(x => x.ReplyId).NotNull().NotEmpty().WithMessage("reply_id cannot be null when liking a Comment");
            RuleFor(x => x.IsLiked).NotEmpty().WithMessage("is_liked can't be null");
        }
    }
}
