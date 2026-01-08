using Asp.Versioning;
using DientesLimpios.API.DTOs.Pacientes;
using DientesLimpios.API.Utilidades;
using DientesLimpios.Aplicacion.CasosdeUso.Pacientes.Comandos.ActualizarPaciente;
using DientesLimpios.Aplicacion.CasosdeUso.Pacientes.Comandos.BorrarPaciente;
using DientesLimpios.Aplicacion.CasosdeUso.Pacientes.Comandos.CrearPaciente;
using DientesLimpios.Aplicacion.CasosdeUso.Pacientes.Consultas.ObtenerDetallePaciente;
using DientesLimpios.Aplicacion.CasosdeUso.Pacientes.Consultas.ObtenerListadoPacientes;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DientesLimpios.API.Controllers
{
    // 1. Change the Route to include the version
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    // 2. Define the version for this controller
    [ApiVersion("1.0")]
    public class PacientesController(IMediator mediator) : ControllerBase
    {

        [HttpGet]
        public async Task<ActionResult<List<PacienteListadoDTO>>> Get([FromQuery] ConsultaObtenerListadoPacientes consulta)
        {
            var resultado = await mediator.Send(consulta);
            HttpContext.InsertarPaginacionEnCabecera(resultado.Total);
            return resultado.Elementos;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PacienteDetalleDTO>> Get(Guid id)
        {
            var consulta = new ConsultaObtenerDetallePaciente() { Id = id };
            var resultado = await mediator.Send(consulta);
            return resultado;
        }

        [HttpPost]
        public async Task<IActionResult> Post(CrearPacienteDTO crearPacienteDTO)
        {
            var comando = new ComandoCrearPaciente { Nombre = crearPacienteDTO.Nombre, Email = crearPacienteDTO.Email };
            await mediator.Send(comando);
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, ActualizarPacienteDTO actualizarPacienteDTO)
        {
            var comando = new ComandoActualizarPaciente
            {
                Id = id,
                Nombre = actualizarPacienteDTO.Nombre,
                Email = actualizarPacienteDTO.Email
            };
            await mediator.Send(comando);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var comando = new ComandoBorrarPaciente { Id = id };
            await mediator.Send(comando);
            return NoContent();
        }

    }
}
