namespace DientesLimpios.Domain.Exceptions
{
    public class BusinessRuleException : Exception
    {
        public BusinessRuleException() { }

        public BusinessRuleException(string mensaje) : base(mensaje) { }

        public BusinessRuleException(string mensaje, Exception innerException)
            : base(mensaje, innerException) { }
    }
}
