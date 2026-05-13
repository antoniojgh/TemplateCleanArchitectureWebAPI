namespace DientesLimpios.Aplicacion.Excepciones
{
    public class ExcepcionDeMediador : Exception
    {
        public ExcepcionDeMediador() { }

        public ExcepcionDeMediador(string mensaje) : base(mensaje) { }

        public ExcepcionDeMediador(string mensaje, Exception innerException)
            : base(mensaje, innerException) { }

    }
}
