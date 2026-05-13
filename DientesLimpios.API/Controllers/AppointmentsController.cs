using Asp.Versioning;
using DientesLimpios.API.DTOs.Appointments;
using DientesLimpios.API.Extensions;
using DientesLimpios.Application.UseCases.Appointments.Commands.CancelAppointment;
using DientesLimpios.Application.UseCases.Appointments.Commands.CompleteAppointment;
using DientesLimpios.Application.UseCases.Appointments.Commands.CreateAppointment;
using DientesLimpios.Application.UseCases.Appointments.Queries.GetAppointmentDetail;
using DientesLimpios.Application.UseCases.Appointments.Queries.GetAppointmentList;
using DientesLimpios.Application.Utilities.Mediator;
using Microsoft.AspNetCore.Mvc;

namespace DientesLimpios.API.Controllers
{
    // 1. Change the Route to include the version
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    // 2. Define the version for this controller
    [ApiVersion("1.0")]
    public class AppointmentsController(IMediator mediator) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] GetAppointmentListQuery query, CancellationToken ct)
        {
            var result = await mediator.Send(query, ct);
            return result.ToActionResult(HttpContext);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id, CancellationToken ct)
        {
            var query = new GetAppointmentDetailQuery { Id = id };

            var result = await mediator.Send(query, ct);
            
            return result.ToActionResult(HttpContext);
        }

        [HttpPost]
        public async Task<IActionResult> Post(CreateAppointmentDTO crearAppointmentDTO, CancellationToken ct)
        {
            var command = new CreateAppointmentCommand
            {
                OfficeId = crearAppointmentDTO.OfficeId,
                DentistId = crearAppointmentDTO.DentistId,
                PatientId = crearAppointmentDTO.PatientId,
                StartDate = crearAppointmentDTO.StartDate,
                EndDate = crearAppointmentDTO.EndDate
            };

            var result = await mediator.Send(command, ct);

            return result.ToActionResult(HttpContext);
        }

        [HttpPost("completar/{id}")]
        public async Task<IActionResult> Complete(Guid id, CancellationToken ct)
        {
            var query = new CompleteAppointmentCommand { Id = id };

            var result = await mediator.Send(query, ct);

            return result.ToActionResult(HttpContext);
        }

        [HttpPost("cancelar/{id}")]
        public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
        {
            var query = new CancelAppointmentCommand { Id = id };

            var result = await mediator.Send(query, ct);
            
            return result.ToActionResult(HttpContext);
        }


    }
}
