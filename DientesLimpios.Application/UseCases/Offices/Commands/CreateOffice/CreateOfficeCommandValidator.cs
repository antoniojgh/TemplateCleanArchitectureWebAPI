using FluentValidation;
using DientesLimpios.Domain.Entities;

namespace DientesLimpios.Application.UseCases.Offices.Commands.CreateOffice
{
    public class CreateOfficeCommandValidator :AbstractValidator<CreateOfficeCommand>
    {
        public CreateOfficeCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("{PropertyName} Field is required")
                .MaximumLength(Office.NameMaxLength).WithMessage("The length of the {PropertyName} field must be less than or equal to {MaxLength}");
        }
    }
}
