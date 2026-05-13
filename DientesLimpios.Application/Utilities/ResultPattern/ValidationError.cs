using DientesLimpios.Domain.Common.ResultPattern;

namespace DientesLimpios.Application.Utilities.ResultPattern
{
    public sealed record ValidationError(Error[] Errors)
        : Error("Validacion.General", "Han ocurrido uno o más errores de validación.");
}
