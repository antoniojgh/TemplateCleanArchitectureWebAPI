namespace DientesLimpios.Dominio.Excepciones
{
    public class ExcepcionDeReglaDeNegocio : Exception
    {
        public ExcepcionDeReglaDeNegocio() { }

        public ExcepcionDeReglaDeNegocio(string mensaje) : base(mensaje) { }

        public ExcepcionDeReglaDeNegocio(string mensaje, Exception innerException)
            : base(mensaje, innerException) { }
    }
}
