using Asp.Versioning;
using DientesLimpios.API.DTOs.Dentists;
using DientesLimpios.API.Extensions;
using DientesLimpios.API.Utilities;
using DientesLimpios.Application.UseCases.Dentists.Commands.UpdateDentist;
using DientesLimpios.Application.UseCases.Dentists.Commands.DeleteDentist;
using DientesLimpios.Application.UseCases.Dentists.Commands.CreateDentist;
using DientesLimpios.Application.UseCases.Dentists.Queries.GetDentistDetail;
using DientesLimpios.Application.UseCases.Dentists.Queries.GetDentistList;
using DientesLimpios.Application.Utilities.Mediator;
using Microsoft.AspNetCore.Mvc;

namespace DientesLimpios.API.Controllers
{
    // 1. Change the Route to include the version
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    // 2. Define the version for this controller
    [ApiVersion("1.0")]
    public class DentistsController(IMediator mediator) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] GetDentistListQuery query, CancellationToken ct)
        {
            var result = await mediator.Send(query, ct);

            if (result.IsSuccess && result.Value != null)
                HttpContext.InsertPaginationInHeader(result.Value.Total);

            return result.ToActionResult(HttpContext);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id, CancellationToken ct)
        {
            var query = new GetDentistDetailQuery() { Id = id };

            var result = await mediator.Send(query, ct);

            return result.ToActionResult(HttpContext);
        }

        [HttpPost]
        public async Task<IActionResult> Post(CreateDentistDTO createDentistDto, CancellationToken ct)
        {
            var command = new CreateDentistCommand { Name = createDentistDto.Name, Email = createDentistDto.Email };

            var result = await mediator.Send(command, ct);

            return result.ToCreatedResult(HttpContext, id => $"/api/v1/dentists/{id}");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, UpdateDentistDTO updateDentistDto, CancellationToken ct)
        {
            var command = new UpdateDentistCommand
            {
                Id = id,
                Name = updateDentistDto.Name,
                Email = updateDentistDto.Email
            };

            var result = await mediator.Send(command, ct);

            return result.ToActionResult(HttpContext);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var command = new DeleteDentistCommand { Id = id };

            var result = await mediator.Send(command, ct);

            return result.ToActionResult(HttpContext);
        }
    }
}
