using DientesLimpios.Aplicacion.Excepciones;
using DientesLimpios.Aplicacion.Interfaces.Repositorios;
using MediatR;

namespace DientesLimpios.Aplicacion.CasosdeUso.Consultorios.Consultas.ObtenerDetalleConsultorio
{
    public class HandlerObtenerDetalleConsultorio(IRepositorioConsultorios repositorio) : IRequestHandler<ConsultaObtenerDetalleConsultorio, ConsultorioDetalleDTO>
    {
        public async Task<ConsultorioDetalleDTO> Handle(ConsultaObtenerDetalleConsultorio request, CancellationToken cancellationToken)
        {
            var consultorio = await repositorio.ObtenerPorId(request.Id);

            if (consultorio is null)
            {
                throw new ExcepcionNoEncontrado();
            }

            var consultorioDetalleDTO = consultorio.ADto();

            return consultorioDetalleDTO;
        }
    }
}
