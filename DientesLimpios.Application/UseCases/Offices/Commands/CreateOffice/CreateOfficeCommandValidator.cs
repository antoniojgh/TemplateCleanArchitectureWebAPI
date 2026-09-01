using FluentValidation;

namespace DientesLimpios.Application.UseCases.Offices.Commands.CreateOffice
{
    public class CreateOfficeCommandValidator :AbstractValidator<CreateOfficeCommand>
    {
        public CreateOfficeCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("{PropertyName} Field is required")
                .MaximumLength(150).WithMessage("The length of the {PropertyName} field must be less than or equal to {MaxLength}");
        }
    }
}
