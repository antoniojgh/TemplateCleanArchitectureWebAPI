namespace DientesLimpios.Domain.Common.ResultPattern
{
    public record Error(string Code, string Message)
    {
        public static readonly Error None = new(string.Empty, string.Empty);
    }

}
