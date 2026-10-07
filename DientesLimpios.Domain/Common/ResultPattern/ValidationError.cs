namespace DientesLimpios.Domain.Common.ResultPattern
{
    public sealed record ValidationError(Error[] Errors)
        : Error("Validation.General", "One or more validation errors occurred.");
}
