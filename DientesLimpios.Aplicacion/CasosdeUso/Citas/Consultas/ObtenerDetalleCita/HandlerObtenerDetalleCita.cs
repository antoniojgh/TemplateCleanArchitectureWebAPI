using DientesLimpios.Aplicacion.Excepciones;
using DientesLimpios.Aplicacion.Interfaces.Repositorios;
using DientesLimpios.Aplicacion.Utilidades.Mediador;

namespace DientesLimpios.Aplicacion.CasosdeUso.Citas.Consultas.ObtenerDetalleCita
{
    public class HandlerObtenerDetalleCita(IRepositorioCitas repositorio) : IRequestHandler<ConsultaObtenerDetalleCita, CitaDetalleDTO>
    {
        public async Task<CitaDetalleDTO> Handle(ConsultaObtenerDetalleCita request)
        {
            var cita = await repositorio.ObtenerPorId(request.Id);

            if (cita is null)
            {
                throw new ExcepcionNoEncontrado();
            }

            var citaDetalleDTO = cita.ADto();

            return citaDetalleDTO;
        }
    }
}
