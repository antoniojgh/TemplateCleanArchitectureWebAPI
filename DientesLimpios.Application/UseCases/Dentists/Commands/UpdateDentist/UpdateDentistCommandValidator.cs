using DientesLimpios.Application.UseCases.Patients.Commands.UpdatePatient;
using FluentValidation;

namespace DientesLimpios.Application.UseCases.Dentists.Commands.UpdateDentist
{
    public class UpdateDentistCommandValidator : AbstractValidator<UpdateDentistCommand>
    {
        public UpdateDentistCommandValidator()
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
