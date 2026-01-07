using DientesLimpios.API.DTOs.Citas;
using DientesLimpios.Aplicacion.CasosdeUso.Citas.Comandos.CompletarCita;
using DientesLimpios.Aplicacion.CasosdeUso.Citas.Comandos.CrearCita;
using DientesLimpios.Aplicacion.CasosdeUso.Citas.Consultas.ObtenerDetalleCita;
using DientesLimpios.Aplicacion.CasosdeUso.Citas.Consultas.ObtenerListadoCitas;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DientesLimpios.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
