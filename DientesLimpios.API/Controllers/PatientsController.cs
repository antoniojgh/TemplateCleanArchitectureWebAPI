using Asp.Versioning;
using DientesLimpios.API.DTOs.Patients;
using DientesLimpios.API.Extensions;
using DientesLimpios.API.Utilities;
using DientesLimpios.Application.UseCases.Patients.Commands.UpdatePatient;
using DientesLimpios.Application.UseCases.Patients.Commands.DeletePatient;
using DientesLimpios.Application.UseCases.Patients.Commands.CreatePatient;
using DientesLimpios.Application.UseCases.Patients.Queries.GetPatientDetail;
using DientesLimpios.Application.UseCases.Patients.Queries.GetPatientList;
using DientesLimpios.Application.Utilities.Mediator;
using Microsoft.AspNetCore.Mvc;

namespace DientesLimpios.API.Controllers
{
    // 1. Change the Route to include the version
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    // 2. Define the version for this controller
    [ApiVersion("1.0")]
    public class PatientsController(IMediator mediator) : ControllerBase
    {

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] GetPatientListQuery query, CancellationToken ct)
        {
            var result = await mediator.Send(query, ct);

            if (result.IsSuccess && result.Value != null)
                HttpContext.InsertPaginationInHeader(result.Value.Total);
            

            return result.ToActionResult(HttpContext);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id, CancellationToken ct)
        {
            var query = new GetPatientDetailQuery() { Id = id };

            var result = await mediator.Send(query, ct);

            return result.ToActionResult(HttpContext);
        }

        [HttpPost]
        public async Task<IActionResult> Post(CreatePatientDTO createPatientDto, CancellationToken ct)
        {
            var command = new CreatePatientCommand { Name = createPatientDto.Name, Email = createPatientDto.Email };

            var result = await mediator.Send(command, ct);

            return result.ToCreatedResult(HttpContext, id => $"/api/v1/patients/{id}");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, UpdatePatientDTO updatePatientDto, CancellationToken ct)
        {
            var command = new UpdatePatientCommand
            {
                Id = id,
                Name = updatePatientDto.Name,
                Email = updatePatientDto.Email
            };

            var result = await mediator.Send(command, ct);

            return result.ToActionResult(HttpContext);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var command = new DeletePatientCommand { Id = id };

            var result = await mediator.Send(command, ct);

            return result.ToActionResult(HttpContext);
        }

    }
}
