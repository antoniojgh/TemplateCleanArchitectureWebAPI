using FluentValidation;

namespace DientesLimpios.Aplicacion.CasosdeUso.Citas.Comandos.CrearCita
{
    public class ValidadorComandoCrearCita : AbstractValidator<ComandoCrearCita>
    {
        public ValidadorComandoCrearCita()
        {
            RuleFor(x => x.FechaInicio)
                .LessThan(x => x.FechaFin).WithMessage("La fecha de inicio debe ser anterior a la fecha de fin")
                .GreaterThan(DateTime.UtcNow).WithMessage("La fecha inicio no puede estar en el pasado");

        }
    }
}
