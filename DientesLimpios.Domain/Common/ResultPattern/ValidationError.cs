namespace DientesLimpios.Domain.Common.ResultPattern
{
    public sealed class ValidationError : Error
    {
        public ValidationError(Error[] errors)
            : base("Validacion.General", "One or more validation errors occurred.")
        {
            Errors = errors;
        }

        public Error[] Errors { get; }
    }
}
