using FluentValidation;

namespace DientesLimpios.Application.UseCases.Offices.Commands.UpdateOffice
{
    public class UpdateOfficeCommandValidator : AbstractValidator<UpdateOfficeCommand>
    {
        public UpdateOfficeCommandValidator()
        {
            RuleFor(p => p.Name)
            .NotEmpty().WithMessage("{PropertyName} Field is required")
            .MaximumLength(150).WithMessage("The length of the {PropertyName} field must be less than or equal to {MaxLength}");
        }
    }
}
