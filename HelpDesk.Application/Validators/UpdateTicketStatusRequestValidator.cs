using FluentValidation;
using HelpDesk.Application.DTOs.Ticket;
using HelpDesk.Domain.Enums;

namespace HelpDesk.Application.Validators;

public class UpdateTicketStatusRequestValidator : AbstractValidator<UpdateTicketStatusRequest>
{
    public UpdateTicketStatusRequestValidator()
    {
        RuleFor(x => x.NewStatus)
            .NotEmpty()
            .Must(s => Enum.TryParse<TicketStatus>(s, out _))
            .WithMessage("NewStatus must be a valid value: Open, InProgress, WaitingForInfo, Closed, Reopened");
    }
}
