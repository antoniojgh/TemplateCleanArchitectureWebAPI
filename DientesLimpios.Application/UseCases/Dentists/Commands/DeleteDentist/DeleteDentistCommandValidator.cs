using FluentValidation;

namespace DientesLimpios.Application.UseCases.Dentists.Commands.DeleteDentist
{
    public class DeleteDentistCommandValidator : AbstractValidator<DeleteDentistCommand>
    {
        public DeleteDentistCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotNull().WithMessage("The {PropertyName} field is required")
                .NotEmpty().WithMessage("The {PropertyName} field must have a valid value");
        }
    }
}
