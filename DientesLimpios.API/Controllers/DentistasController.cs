using Asp.Versioning;
using DientesLimpios.API.DTOs.Dentistas;
using DientesLimpios.API.Extensiones;
using DientesLimpios.API.Utilidades;
using DientesLimpios.Aplicacion.CasosdeUso.Dentistas.Comandos.ActualizarDentista;
using DientesLimpios.Aplicacion.CasosdeUso.Dentistas.Comandos.BorrarDentista;
using DientesLimpios.Aplicacion.CasosdeUso.Dentistas.Comandos.CrearDentista;
using DientesLimpios.Aplicacion.CasosdeUso.Dentistas.Consultas.ObtenerDetalleDentista;
using DientesLimpios.Aplicacion.CasosdeUso.Dentistas.Consultas.ObtenerListadoDentistas;
using DientesLimpios.Aplicacion.Utilidades.Mediador;
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
        public async Task<IActionResult> Get([FromQuery] ConsultaObtenerListadoDentistas consulta, CancellationToken ct)
        {
            var result = await mediator.Send(consulta, ct);

            HttpContext.InsertarPaginacionEnCabecera(result.Value.Total);

            return result.ToActionResult(HttpContext);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id, CancellationToken ct)
        {
            var consulta = new ConsultaObtenerDetalleDentista() { Id = id };

            var result = await mediator.Send(consulta, ct);

            return result.ToActionResult(HttpContext);
        }

        [HttpPost]
        public async Task<IActionResult> Post(CrearDentistaDTO crearDentistaDTO, CancellationToken ct)
        {
            var comando = new ComandoCrearDentista { Nombre = crearDentistaDTO.Nombre, Email = crearDentistaDTO.Email };

            var result = await mediator.Send(comando, ct);

            return result.ToActionResult(HttpContext);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, ActualizarDentistaDTO actualizarDentistaDTO, CancellationToken ct)
        {
            var comando = new ComandoActualizarDentista
            {
                Id = id,
                Nombre = actualizarDentistaDTO.Nombre,
                Email = actualizarDentistaDTO.Email
            };

            var result = await mediator.Send(comando, ct);

            return result.ToActionResult(HttpContext);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var comando = new ComandoBorrarDentista { Id = id };

            var result = await mediator.Send(comando, ct);

            return result.ToActionResult(HttpContext);
        }
    }
}
