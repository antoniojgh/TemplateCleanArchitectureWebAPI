using FluentValidation;

namespace DientesLimpios.Aplicacion.CasosdeUso.Pacientes.Comandos.BorrarPaciente
{
    public class ValidadorComandoBorrarPaciente : AbstractValidator<ComandoBorrarPaciente>
    {
        public ValidadorComandoBorrarPaciente()
        {
            RuleFor(x => x.Id)
                .NotNull().WithMessage("El campo {Id} es requerido")
                .NotEmpty().WithMessage("El campo {Id} debe tener un valor válido");
        }
    }
}
