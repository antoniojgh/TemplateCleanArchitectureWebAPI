using DientesLimpios.Aplicacion.Interfaces.Repositorios;
using DientesLimpios.Aplicacion.Utilidades.Mediador;
using DientesLimpios.Dominio.Comunes.PatronResultados;
using DientesLimpios.Dominio.Errores;
using Microsoft.Extensions.Logging;


namespace DientesLimpios.Aplicacion.CasosdeUso.Citas.Consultas.ObtenerDetalleCita
{
    public class HandlerObtenerDetalleCita(IRepositorioCitas repositorio, ILogger<HandlerObtenerDetalleCita> logger) : IRequestHandler<ConsultaObtenerDetalleCita, Result<CitaDetalleDTO>>
    {
        public async Task<Result<CitaDetalleDTO>> Handle(ConsultaObtenerDetalleCita request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Obteniendo detalle de cita con ID: {CitaId}", request.Id);

            var cita = await repositorio.ObtenerPorId(request.Id);

            if (cita is null)
                return Result.Failure<CitaDetalleDTO>(DomainErrors.Cita.NoEncontrada);

            logger.LogInformation("Detalle de cita obtenido correctamente con ID: {CitaId}", request.Id);

            return Result.Success(cita.ADto());
        }
    }
}
