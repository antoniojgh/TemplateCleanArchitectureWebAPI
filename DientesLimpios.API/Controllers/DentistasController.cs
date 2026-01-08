using Asp.Versioning;
using DientesLimpios.API.DTOs.Dentistas;
using DientesLimpios.API.Utilidades;
using DientesLimpios.Aplicacion.CasosdeUso.Dentistas.Comandos.ActualizarDentista;
using DientesLimpios.Aplicacion.CasosdeUso.Dentistas.Comandos.BorrarDentista;
using DientesLimpios.Aplicacion.CasosdeUso.Dentistas.Comandos.CrearDentista;
using DientesLimpios.Aplicacion.CasosdeUso.Dentistas.Consultas.ObtenerDetalleDentista;
using DientesLimpios.Aplicacion.CasosdeUso.Dentistas.Consultas.ObtenerListadoDentistas;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DientesLimpios.API.Controllers
{
    // 1. Change the Route to include the version
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    // 2. Define the version for this controller
    [ApiVersion("1.0")]
    public class DentistasController(IMediator mediator) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<DentistaListadoDTO>>> Get([FromQuery] ConsultaObtenerListadoDentistas consulta)
        {
            var resultado = await mediator.Send(consulta);
            HttpContext.InsertarPaginacionEnCabecera(resultado.Total);
            return resultado.Elementos;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DentistaDetalleDTO>> Get(Guid id)
        {
            var consulta = new ConsultaObtenerDetalleDentista() { Id = id };
            var resultado = await mediator.Send(consulta);
            return resultado;
        }

        [HttpPost]
        public async Task<IActionResult> Post(CrearDentistaDTO crearDentistaDTO)
        {
            var comando = new ComandoCrearDentista { Nombre = crearDentistaDTO.Nombre, Email = crearDentistaDTO.Email };
            await mediator.Send(comando);
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, ActualizarDentistaDTO actualizarDentistaDTO)
        {
            var comando = new ComandoActualizarDentista
            {
                Id = id,
                Nombre = actualizarDentistaDTO.Nombre,
                Email = actualizarDentistaDTO.Email
            };
            await mediator.Send(comando);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var comando = new ComandoBorrarDentista { Id = id };
            await mediator.Send(comando);
            return NoContent();
        }
    }
}
