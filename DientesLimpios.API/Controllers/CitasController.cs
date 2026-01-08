using DientesLimpios.API.DTOs.Citas;
using DientesLimpios.Aplicacion.CasosdeUso.Citas.Comandos.CompletarCita;
using DientesLimpios.Aplicacion.CasosdeUso.Citas.Comandos.CrearCita;
using DientesLimpios.Aplicacion.CasosdeUso.Citas.Consultas.ObtenerDetalleCita;
using DientesLimpios.Aplicacion.CasosdeUso.Citas.Consultas.ObtenerListadoCitas;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

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
        public async Task<ActionResult<List<CitaListadoDTO>>> Get([FromQuery] ConsultaObtenerListadoCitas consulta)
        {
            var resultado = await mediator.Send(consulta);
            return resultado;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CitaDetalleDTO>> Get(Guid id)
        {
            var consulta = new ConsultaObtenerDetalleCita { Id = id };
            var resultado = await mediator.Send(consulta);
            return resultado;
        }

        [HttpPost]
        public async Task<IActionResult> Post(CrearCitaDTO crearCitaDTO)
        {
            var comando = new ComandoCrearCita
            {
                ConsultorioId = crearCitaDTO.ConsultorioId,
                DentistaId = crearCitaDTO.DentistaId,
                PacienteId = crearCitaDTO.PacienteId,
                FechaInicio = crearCitaDTO.FechaInicio,
                FechaFin = crearCitaDTO.FechaFin
            };

            var resultado = await mediator.Send(comando);
            return Ok();
        }

        [HttpPost("completar/{id}")]
        public async Task<IActionResult> Completar(Guid id)
        {
            var consulta = new ComandoCompletarCita { Id = id };
            await mediator.Send(consulta);
            return Ok();
        }

        [HttpPost("cancelar/{id}")]
        public async Task<IActionResult> Cancelar(Guid id)
        {
            var consulta = new ComandoCancelarCita { Id = id };
            await mediator.Send(consulta);
            return Ok();
        }


    }
}
