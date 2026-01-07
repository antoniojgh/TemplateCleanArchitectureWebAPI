using FluentValidation;

namespace DientesLimpios.Aplicacion.CasosdeUso.Dentistas.Comandos.BorrarDentista
{
    public class ValidadorComandoBorrarDentista : AbstractValidator<ComandoBorrarDentista>
    {
        public ValidadorComandoBorrarDentista()
        {
            RuleFor(x => x.Id)
                .NotNull().WithMessage("El campo {Id} es requerido")
                .NotEmpty().WithMessage("El campo {Id} debe tener un valor válido");
        }
    }
}
