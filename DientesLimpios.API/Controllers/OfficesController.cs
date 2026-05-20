using Asp.Versioning;
using DientesLimpios.API.DTOs.Offices;
using DientesLimpios.API.Extensions;
using DientesLimpios.Application.UseCases.Offices.Commands.UpdateOffice;
using DientesLimpios.Application.UseCases.Offices.Commands.DeleteOffice;
using DientesLimpios.Application.UseCases.Offices.Commands.CreateOffice;
using DientesLimpios.Application.UseCases.Offices.Queries.GetOfficeDetail;
using DientesLimpios.Application.UseCases.Offices.Queries.GetOfficeList;
using DientesLimpios.Application.Utilities.Mediator;
using Microsoft.AspNetCore.Mvc;

namespace DientesLimpios.API.Controllers
{
    // 1. Change the Route to include the version
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    // 2. Define the version for this controller
    [ApiVersion("1.0")]
    public class OfficesController(IMediator mediator) : ControllerBase
    {

        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken ct)
        {
            var query = new GetOfficeListQuery();

            var result = await mediator.Send(query, ct);

            return result.ToActionResult(HttpContext);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id, CancellationToken ct)
        {
            var query = new GetOfficeDetailQuery { Id = id };

            var result = await mediator.Send(query, ct);

            return result.ToActionResult(HttpContext);
        }


        [HttpPost]
        public async Task<IActionResult> Post(CreateOfficeDTO createOfficeDto, CancellationToken ct)
        {
            var command = new CreateOfficeCommand { Name = createOfficeDto.Name };

            var result = await mediator.Send(command, ct);

            return result.ToCreatedResult(HttpContext, id => $"/api/v1/offices/{id}");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, UpdateOfficeDTO updateOfficeDto, CancellationToken ct)
        {
            var command = new UpdateOfficeCommand { Id = id, Name = updateOfficeDto.Name };

            var result = await mediator.Send(command, ct);

            return result.ToActionResult(HttpContext);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var command = new DeleteOfficeCommand { Id = id};

            var result = await mediator.Send(command, ct);

            return result.ToActionResult(HttpContext);
        }
    }
}
