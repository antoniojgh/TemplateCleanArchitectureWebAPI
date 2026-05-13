namespace DientesLimpios.Application.Exceptions
{
    public class MediatorException : Exception
    {
        public MediatorException() { }

        public MediatorException(string mensaje) : base(mensaje) { }

        public MediatorException(string mensaje, Exception innerException)
            : base(mensaje, innerException) { }

    }
}
