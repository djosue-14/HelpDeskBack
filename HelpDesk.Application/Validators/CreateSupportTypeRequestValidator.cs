using FluentValidation;
using HelpDesk.Application.DTOs.SupportType;

namespace HelpDesk.Application.Validators;

public class CreateSupportTypeRequestValidator : AbstractValidator<CreateSupportTypeRequest>
{
    public CreateSupportTypeRequestValidator()
    {
        RuleFor(x => x.DepartmentId)
            .NotEmpty()
            .WithMessage("DepartmentId is required");

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(255)
            .WithMessage("Name is required and must not exceed 255 characters");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required");
    }
}
