using DientesLimpios.Dominio.Comunes.PatronResultados;

namespace DientesLimpios.Aplicacion.Utilidades.PatronResultados
{
    public sealed record ValidationError(Error[] Errors)
        : Error("Validacion.General", "Han ocurrido uno o más errores de validación.");
}
