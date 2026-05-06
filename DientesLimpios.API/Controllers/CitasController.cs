using Asp.Versioning;
using DientesLimpios.API.DTOs.Citas;
using DientesLimpios.API.Extensiones;
using DientesLimpios.Aplicacion.CasosdeUso.Citas.Comandos.CancelarCita;
using DientesLimpios.Aplicacion.CasosdeUso.Citas.Comandos.CompletarCita;
using DientesLimpios.Aplicacion.CasosdeUso.Citas.Comandos.CrearCita;
using DientesLimpios.Aplicacion.CasosdeUso.Citas.Consultas.ObtenerDetalleCita;
using DientesLimpios.Aplicacion.CasosdeUso.Citas.Consultas.ObtenerListadoCitas;
using DientesLimpios.Aplicacion.Utilidades.Mediador;
using Microsoft.AspNetCore.Mvc;

namespace DientesLimpios.API.Controllers
{
    // 1. Change the Route to include the version
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    // 2. Define the version for this controller
    [ApiVersion("1.0")]
    public class CitasController(IMediator mediator) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] ConsultaObtenerListadoCitas consulta, CancellationToken ct)
        {
            var result = await mediator.Send(consulta, ct);
            return result.ToActionResult(HttpContext);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id, CancellationToken ct)
        {
            var consulta = new ConsultaObtenerDetalleCita { Id = id };

            var result = await mediator.Send(consulta, ct);
            
            return result.ToActionResult(HttpContext);
        }

        [HttpPost]
        public async Task<IActionResult> Post(CrearCitaDTO crearCitaDTO, CancellationToken ct)
        {
            var comando = new ComandoCrearCita
            {
                ConsultorioId = crearCitaDTO.ConsultorioId,
                DentistaId = crearCitaDTO.DentistaId,
                PacienteId = crearCitaDTO.PacienteId,
                FechaInicio = crearCitaDTO.FechaInicio,
                FechaFin = crearCitaDTO.FechaFin
            };

            var result = await mediator.Send(comando, ct);

            return result.ToActionResult(HttpContext);
        }

        [HttpPost("completar/{id}")]
        public async Task<IActionResult> Completar(Guid id, CancellationToken ct)
        {
            var consulta = new ComandoCompletarCita { Id = id };

            var result = await mediator.Send(consulta, ct);

            return result.ToActionResult(HttpContext);
        }

        [HttpPost("cancelar/{id}")]
        public async Task<IActionResult> Cancelar(Guid id, CancellationToken ct)
        {
            var consulta = new ComandoCancelarCita { Id = id };

            var result = await mediator.Send(consulta, ct);
            
            return result.ToActionResult(HttpContext);
        }


    }
}
