using Asp.Versioning;
using DientesLimpios.API.DTOs.Pacientes;
using DientesLimpios.API.Extensiones;
using DientesLimpios.API.Utilidades;
using DientesLimpios.Aplicacion.CasosdeUso.Pacientes.Comandos.ActualizarPaciente;
using DientesLimpios.Aplicacion.CasosdeUso.Pacientes.Comandos.BorrarPaciente;
using DientesLimpios.Aplicacion.CasosdeUso.Pacientes.Comandos.CrearPaciente;
using DientesLimpios.Aplicacion.CasosdeUso.Pacientes.Consultas.ObtenerDetallePaciente;
using DientesLimpios.Aplicacion.CasosdeUso.Pacientes.Consultas.ObtenerListadoPacientes;
using DientesLimpios.Aplicacion.Utilidades.Mediador;
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
        public async Task<IActionResult> Get([FromQuery] ConsultaObtenerListadoPacientes consulta, CancellationToken ct)
        {
            var result = await mediator.Send(consulta, ct);

            HttpContext.InsertarPaginacionEnCabecera(result.Value.Total);

            return result.ToActionResult(HttpContext);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id, CancellationToken ct)
        {
            var consulta = new ConsultaObtenerDetallePaciente() { Id = id };

            var result = await mediator.Send(consulta, ct);

            return result.ToActionResult(HttpContext);
        }

        [HttpPost]
        public async Task<IActionResult> Post(CrearPacienteDTO crearPacienteDTO, CancellationToken ct)
        {
            var comando = new ComandoCrearPaciente { Nombre = crearPacienteDTO.Nombre, Email = crearPacienteDTO.Email };

            var result = await mediator.Send(comando, ct);

            return result.ToActionResult(HttpContext);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, ActualizarPacienteDTO actualizarPacienteDTO, CancellationToken ct)
        {
            var comando = new ComandoActualizarPaciente
            {
                Id = id,
                Nombre = actualizarPacienteDTO.Nombre,
                Email = actualizarPacienteDTO.Email
            };

            var result = await mediator.Send(comando, ct);

            return result.ToActionResult(HttpContext);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var comando = new ComandoBorrarPaciente { Id = id };

            var result = await mediator.Send(comando, ct);

            return result.ToActionResult(HttpContext);
        }

    }
}
