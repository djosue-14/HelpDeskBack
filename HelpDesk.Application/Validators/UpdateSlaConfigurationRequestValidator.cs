using FluentValidation;
using HelpDesk.Application.DTOs.SlaConfiguration;

namespace HelpDesk.Application.Validators;

public class UpdateSlaConfigurationRequestValidator : AbstractValidator<UpdateSlaConfigurationRequest>
{
    public UpdateSlaConfigurationRequestValidator()
    {
        RuleFor(x => x.HoursLimit)
            .GreaterThan(0)
            .WithMessage("HoursLimit must be a positive integer greater than zero");
    }
}
