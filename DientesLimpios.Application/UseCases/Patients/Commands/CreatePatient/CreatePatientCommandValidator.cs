using FluentValidation;
using DientesLimpios.Domain.Entities;
using DientesLimpios.Domain.ValueObjects;

namespace DientesLimpios.Application.UseCases.Patients.Commands.CreatePatient
{
    public class CreatePatientCommandValidator : AbstractValidator<CreatePatientCommand>
    {
        public CreatePatientCommandValidator()
        {
            RuleFor(p => p.Name)
        .NotEmpty().WithMessage("The {PropertyName} field is required")
        .MaximumLength(Patient.NameMaxLength).WithMessage("The length of the {PropertyName} field must be less than or equal to {MaxLength}");

            RuleFor(p => p.Email)
        .NotEmpty().WithMessage("The {PropertyName} field is required")
        .MaximumLength(Email.MaxLength).WithMessage("The length of the {PropertyName} field must be less than or equal to {MaxLength}")
        .EmailAddress().WithMessage("The {PropertyName} field must be a valid email address");

        }
    }
}
