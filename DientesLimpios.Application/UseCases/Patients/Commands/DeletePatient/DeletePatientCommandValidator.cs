using FluentValidation;

namespace DientesLimpios.Application.UseCases.Patients.Commands.DeletePatient
{
    public class DeletePatientCommandValidator : AbstractValidator<DeletePatientCommand>
    {
        public DeletePatientCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotNull().WithMessage("The {PropertyName} field is required")
                .NotEmpty().WithMessage("The {PropertyName} field must have a valid value");
        }
    }
}
