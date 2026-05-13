using FluentValidation;

namespace DientesLimpios.Application.UseCases.Offices.Commands.DeleteOffice
{
    public class DeleteOfficeCommandValidator : AbstractValidator<DeleteOfficeCommand>
    {
        public DeleteOfficeCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotNull().WithMessage("El campo {Id} es requerido")
                .NotEmpty().WithMessage("El campo {Id} debe tener un valor válido");
        }
    }
}
