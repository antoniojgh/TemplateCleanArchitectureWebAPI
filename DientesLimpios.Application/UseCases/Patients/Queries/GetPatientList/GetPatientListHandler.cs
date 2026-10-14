using DientesLimpios.Application.Interfaces.Persistence;
using DientesLimpios.Application.Interfaces.Repositories;
using DientesLimpios.Application.UseCases.Dentists.Queries.GetDentistList;
using DientesLimpios.Application.Utilities.Common;
using DientesLimpios.Application.Utilities.Mediator;
using DientesLimpios.Domain.Common.ResultPattern;
using Microsoft.Extensions.Logging;


namespace DientesLimpios.Application.UseCases.Patients.Queries.GetPatientList
{
    public class GetPatientListHandler(IApplicationDbContext db, ILogger<GetPatientListHandler> logger) : IRequestHandler<GetPatientListQuery, Result<PagedDTO<PatientListDTO>>>
    {
        public async Task<Result<PagedDTO<PatientListDTO>>> Handle(GetPatientListQuery request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Retrieving patient list");

            var patients = db.Patients.AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Name))
                patients = patients.Where(d => d.Name.Contains(request.Name));

            if (!string.IsNullOrWhiteSpace(request.Email))
                patients = patients.Where(d => d.Email.Value.Contains(request.Email));

            var page = await patients
                .OrderBy(d => d.Name)
                .ThenBy(d => d.Id)          // tie-breaker: stable pages when two patients share a name
                .Select(d => new PatientListDTO { Id = d.Id, Name = d.Name, Email = d.Email.Value })
                .ToPagedAsync(request.Page, request.RecordsPerPage, cancellationToken);

            logger.LogInformation("Patient list retrieved successfully with {PatientCount} patients", page.Items.Count);

            return Result.Success(page);
        }
    }
}
