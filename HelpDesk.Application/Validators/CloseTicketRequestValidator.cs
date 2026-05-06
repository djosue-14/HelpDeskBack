using FluentValidation;
using HelpDesk.Application.DTOs.Ticket;
using HelpDesk.Domain.Enums;

namespace HelpDesk.Application.Validators;

public class CloseTicketRequestValidator : AbstractValidator<CloseTicketRequest>
{
    public CloseTicketRequestValidator()
    {
        RuleFor(x => x.ResolutionCategory)
            .NotEmpty()
            .Must(r => Enum.TryParse<ResolutionCategory>(r, out _))
            .WithMessage("ResolutionCategory must be a valid value: Resolved, Rejected, Duplicate, ClosedNoResponse");
    }
}
