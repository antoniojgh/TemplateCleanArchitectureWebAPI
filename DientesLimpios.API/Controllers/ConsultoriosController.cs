using Asp.Versioning;
using DientesLimpios.API.DTOs.Consultorios;
using DientesLimpios.API.Extensiones;
using DientesLimpios.Aplicacion.CasosdeUso.Consultorios.Comandos.ActualizarConsultorio;
using DientesLimpios.Aplicacion.CasosdeUso.Consultorios.Comandos.BorrarConsultorio;
using DientesLimpios.Aplicacion.CasosdeUso.Consultorios.Comandos.CrearConsultorio;
using DientesLimpios.Aplicacion.CasosdeUso.Consultorios.Consultas.ObtenerDetalleConsultorio;
using DientesLimpios.Aplicacion.CasosdeUso.Consultorios.Consultas.ObtenerListadoConsultorios;
using DientesLimpios.Aplicacion.Utilidades.Mediador;
using Microsoft.AspNetCore.Mvc;

namespace DientesLimpios.API.Controllers
{
    // 1. Change the Route to include the version
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    // 2. Define the version for this controller
    [ApiVersion("1.0")]
    public class ConsultoriosController(IMediator mediator) : ControllerBase
    {

        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken ct)
        {
            var consulta = new ConsultaObtenerListadoConsultorios();

            var result = await mediator.Send(consulta, ct);

            return result.ToActionResult(HttpContext);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id, CancellationToken ct)
        {
            var consulta = new ConsultaObtenerDetalleConsultorio { Id = id };

            var result = await mediator.Send(consulta, ct);

            return result.ToActionResult(HttpContext);
        }


        [HttpPost]
        public async Task<IActionResult> Post(CrearConsultorioDTO crearConsultorioDTO, CancellationToken ct)
        {
            var comando = new ComandoCrearConsultorio { Nombre = crearConsultorioDTO.Nombre };

            var result = await mediator.Send(comando, ct);

            return result.ToActionResult(HttpContext);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, ActualizarConsultorioDTO actualizarConsultorioDTO, CancellationToken ct)
        {
            var comando = new ComandoActualizarConsultorio { Id = id, Nombre = actualizarConsultorioDTO.Nombre };

            var result = await mediator.Send(comando, ct);

            return result.ToActionResult(HttpContext);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var comando = new ComandoBorrarConsultorio { Id = id};

            var result = await mediator.Send(comando, ct);

            return result.ToActionResult(HttpContext);
        }
    }
}
