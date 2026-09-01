using FluentValidation;

namespace DientesLimpios.Application.UseCases.Dentists.Commands.UpdateDentist
{
    public class UpdateDentistCommandValidator : AbstractValidator<UpdateDentistCommand>
    {
        public UpdateDentistCommandValidator()
        {
            RuleFor(p => p.Name)
                .NotEmpty().WithMessage("The {PropertyName} field is required")
                .MaximumLength(250).WithMessage("The length of the {PropertyName} field must be less than or equal to {MaxLength}");

            RuleFor(p => p.Email)
                .NotEmpty().WithMessage("The {PropertyName} field is required")
                .MaximumLength(254).WithMessage("The length of the {PropertyName} field must be less than or equal to {MaxLength}")
                .EmailAddress().WithMessage("The {PropertyName} field is not a valid email address");

        }
    }
}
