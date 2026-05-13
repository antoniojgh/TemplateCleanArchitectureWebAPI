using FluentValidation;

namespace DientesLimpios.Application.UseCases.Patients.Commands.UpdatePatient
{
    public class UpdatePatientCommandValidator : AbstractValidator<UpdatePatientCommand>
    {
        public UpdatePatientCommandValidator()
        {
            RuleFor(p => p.Name)
                .NotEmpty().WithMessage("El campo {PropertyName} es requerido")
                .MaximumLength(250).WithMessage("La lontigud del campo {PropertyName} debe ser menor o igual a {MaxLength}");

            RuleFor(p => p.Email)
                .NotEmpty().WithMessage("El campo {PropertyName} es requerido")
                .MaximumLength(254).WithMessage("La lontigud del campo {PropertyName} debe ser menor o igual a {MaxLength}")
                .EmailAddress().WithMessage("El formato del email no es válido");

        }
    }
}
