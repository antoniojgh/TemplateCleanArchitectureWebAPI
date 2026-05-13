using FluentValidation;

namespace DientesLimpios.Application.UseCases.Dentists.Commands.DeleteDentist
{
    public class DeleteDentistCommandValidator : AbstractValidator<DeleteDentistCommand>
    {
        public DeleteDentistCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotNull().WithMessage("El campo {Id} es requerido")
                .NotEmpty().WithMessage("El campo {Id} debe tener un valor válido");
        }
    }
}
