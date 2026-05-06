using DientesLimpios.Aplicacion.Interfaces.Repositorios;
using DientesLimpios.Aplicacion.Utilidades.Mediador;
using DientesLimpios.Dominio.Comunes.PatronResultados;
using DientesLimpios.Dominio.Errores;
using Microsoft.Extensions.Logging;

namespace DientesLimpios.Aplicacion.CasosdeUso.Consultorios.Consultas.ObtenerDetalleConsultorio
{
    public class HandlerObtenerDetalleConsultorio(IRepositorioConsultorios repositorio, ILogger<HandlerObtenerDetalleConsultorio> logger) : IRequestHandler<ConsultaObtenerDetalleConsultorio, Result<ConsultorioDetalleDTO>>
    {
        public async Task<Result<ConsultorioDetalleDTO>> Handle(ConsultaObtenerDetalleConsultorio request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Obteniendo detalle de consultorio con ID: {ConsultorioId}", request.Id);

            var consultorio = await repositorio.ObtenerPorId(request.Id);

            if (consultorio is null)
                return Result.Failure<ConsultorioDetalleDTO>(DomainErrors.Consultorio.NoEncontrado);

            logger.LogInformation("Detalle de consultorio obtenido correctamente con ID: {ConsultorioId}", request.Id);

            return Result.Success(consultorio.ADto());
        }
    }
}
