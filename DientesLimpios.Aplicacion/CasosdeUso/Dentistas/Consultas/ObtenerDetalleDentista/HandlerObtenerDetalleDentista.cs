using DientesLimpios.Aplicacion.Interfaces.Repositorios;
using DientesLimpios.Aplicacion.Utilidades.Mediador;
using DientesLimpios.Dominio.Comunes.PatronResultados;
using DientesLimpios.Dominio.Errores;
using Microsoft.Extensions.Logging;

namespace DientesLimpios.Aplicacion.CasosdeUso.Dentistas.Consultas.ObtenerDetalleDentista
{
    public class HandlerObtenerDetalleDentista(IRepositorioDentistas repositorio, ILogger<HandlerObtenerDetalleDentista> logger) : IRequestHandler<ConsultaObtenerDetalleDentista, Result<DentistaDetalleDTO>>
    {
        public async Task<Result<DentistaDetalleDTO>> Handle(ConsultaObtenerDetalleDentista request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Obteniendo detalle de dentista con ID: {DentistaId}", request.Id);

            var dentista = await repositorio.ObtenerPorId(request.Id);

            if (dentista is null)
                return Result.Failure<DentistaDetalleDTO>(DomainErrors.Dentista.NoEncontrado);

            logger.LogInformation("Detalle de dentista obtenido correctamente con ID: {DentistaId}", request.Id);

            return Result.Success(dentista.ADto());
        }
    }
}
