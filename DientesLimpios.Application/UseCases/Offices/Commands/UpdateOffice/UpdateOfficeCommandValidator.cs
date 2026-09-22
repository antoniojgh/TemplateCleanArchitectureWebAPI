using FluentValidation;
using DientesLimpios.Domain.Entities;

namespace DientesLimpios.Application.UseCases.Offices.Commands.UpdateOffice
{
    public class UpdateOfficeCommandValidator : AbstractValidator<UpdateOfficeCommand>
    {
        public UpdateOfficeCommandValidator()
        {
            RuleFor(p => p.Name)
            .NotEmpty().WithMessage("{PropertyName} Field is required")
            .MaximumLength(Office.NameMaxLength).WithMessage("The length of the {PropertyName} field must be less than or equal to {MaxLength}");
        }
    }
}
