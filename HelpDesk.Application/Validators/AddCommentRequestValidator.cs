using FluentValidation;
using HelpDesk.Application.DTOs.TicketComment;
using HelpDesk.Domain.Enums;

namespace HelpDesk.Application.Validators;

public class AddCommentRequestValidator : AbstractValidator<AddCommentRequest>
{
    public AddCommentRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Comment content is required");

        RuleFor(x => x.Visibility)
            .NotEmpty()
            .Must(v => Enum.TryParse<CommentVisibility>(v, out _))
            .WithMessage("Visibility must be Public or Internal");
    }
}
