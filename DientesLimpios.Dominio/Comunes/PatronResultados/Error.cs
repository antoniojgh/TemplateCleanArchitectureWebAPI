namespace DientesLimpios.Dominio.Comunes.PatronResultados
{
    public record Error(string Code, string Message)
    {
        public static readonly Error None = new(string.Empty, string.Empty);
        public static readonly Error NullValue = new(
            "Error.NullValue",
            "El valor del resultado especificado es nulo.");

        public static implicit operator string(Error error) => error.Code;
    }

}
