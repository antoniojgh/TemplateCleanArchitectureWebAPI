using FluentValidation;
using DientesLimpios.Domain.Entities;
using DientesLimpios.Domain.ValueObjects;

namespace DientesLimpios.Application.UseCases.Dentists.Commands.CreateDentist
{
    public class CreateDentistCommandValidator : AbstractValidator<CreateDentistCommand>
    {
        public CreateDentistCommandValidator()
        {
            RuleFor(p => p.Name)
        .NotEmpty().WithMessage("The {PropertyName} field is required")
        .MaximumLength(Dentist.NameMaxLength).WithMessage("The length of the {PropertyName} field must be less than or equal to {MaxLength}");

            RuleFor(p => p.Email)
        .NotEmpty().WithMessage("The {PropertyName} field is required")
        .MaximumLength(Email.MaxLength).WithMessage("The length of the {PropertyName} field must be less than or equal to {MaxLength}")
        .EmailAddress().WithMessage("The {PropertyName} field is not a valid email address");

        }
    }
}
