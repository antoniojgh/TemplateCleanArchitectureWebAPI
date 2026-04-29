using DientesLimpios.Dominio.Entidades;

namespace DientesLimpios.Aplicacion.CasosdeUso.Dentistas.Consultas.ObtenerDetalleDentista
{
    public static class MapeadorExtensions
    {
        public static DentistaDetalleDTO ADto(this Dentista dentista)
        {
            var dto = new DentistaDetalleDTO { Id = dentista.Id, Nombre = dentista.Nombre, Email = dentista.Email.Valor };
            return dto;
        }
    }
}
