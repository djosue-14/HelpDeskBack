using FluentValidation;
using HelpDesk.Application.DTOs.SupportTypeAgent;

namespace HelpDesk.Application.Validators;

public class AssignAgentRequestValidator : AbstractValidator<AssignAgentRequest>
{
    public AssignAgentRequestValidator()
    {
        RuleFor(x => x.SupportTypeId)
            .NotEmpty()
            .WithMessage("SupportTypeId is required");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required");
    }
}
