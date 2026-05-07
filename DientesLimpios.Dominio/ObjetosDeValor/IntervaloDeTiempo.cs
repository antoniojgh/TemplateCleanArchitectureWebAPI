using DientesLimpios.Dominio.Comunes.PatronResultados;
using DientesLimpios.Dominio.Errores;

namespace DientesLimpios.Dominio.ObjetosDeValor
{
    public sealed record IntervaloDeTiempo
    {
        public DateTime Inicio { get; private init; }
        public DateTime Fin { get; private init; }

        private IntervaloDeTiempo() { }   // EF Core

        private IntervaloDeTiempo(DateTime inicio, DateTime fin)
        {
            Inicio = inicio;
            Fin = fin;
        }

        public static Result<IntervaloDeTiempo> Crear(DateTime inicio, DateTime fin)
        {
            if (inicio >= fin)
                return Result.Failure<IntervaloDeTiempo>(
                    DomainErrors.IntervaloDeTiempo.InicioMayorOIgualAFin);

            return Result.Success(new IntervaloDeTiempo(inicio, fin));
        }
    }

}
