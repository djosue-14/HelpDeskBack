using FluentValidation;
using HelpDesk.Application.DTOs.Ticket;
using HelpDesk.Domain.Enums;

namespace HelpDesk.Application.Validators;

public class CreateTicketRequestValidator : AbstractValidator<CreateTicketRequest>
{
    public CreateTicketRequestValidator()
    {
        RuleFor(x => x.DepartmentId)
            .NotEmpty()
            .WithMessage("Department is required");

        RuleFor(x => x.SupportTypeId)
            .NotEmpty()
            .WithMessage("Support type is required");

        RuleFor(x => x.Priority)
            .NotEmpty()
            .Must(p => Enum.TryParse<TicketPriority>(p, out _))
            .WithMessage("Priority must be a valid value: Critical, High, Medium, Low");

        RuleFor(x => x.Subject)
            .NotEmpty()
            .MaximumLength(255)
            .WithMessage("Subject is required and must not exceed 255 characters");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required");
    }
}
