using FluentValidation;

namespace DientesLimpios.Aplicacion.CasosdeUso.Consultorios.Comandos.BorrarConsultorio
{
    public class ValidadorComandoBorrarConsultorio : AbstractValidator<ComandoBorrarConsultorio>
    {
        public ValidadorComandoBorrarConsultorio()
        {
            RuleFor(x => x.Id)
                .NotNull().WithMessage("El campo {Id} es requerido")
                .NotEmpty().WithMessage("El campo {Id} debe tener un valor válido");
        }
    }
}
