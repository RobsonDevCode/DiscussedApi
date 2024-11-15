using Discusseddto.CommentDtos.ReplyDtos;
using FluentValidation;

namespace DiscussedApi.Validations.Reply
{
    public class PostCommentRequestValidation : AbstractValidator<PostReplyDto>
    {
        public PostCommentRequestValidation()
        {
            RuleFor(x => x.CommentId).NotNull().NotEmpty();
        }
    }
}
