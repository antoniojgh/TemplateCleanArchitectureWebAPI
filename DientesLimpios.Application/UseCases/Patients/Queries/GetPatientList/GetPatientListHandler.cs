using DientesLimpios.Application.Interfaces.Persistence;
using DientesLimpios.Application.Interfaces.Repositories;
using DientesLimpios.Application.Utilities.Common;
using DientesLimpios.Application.Utilities.Mediator;
using DientesLimpios.Domain.Common.ResultPattern;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;


namespace DientesLimpios.Application.UseCases.Patients.Queries.GetPatientList
{
    public class GetPatientListHandler(IPatientRepository repository, IApplicationDbContext db, ILogger<GetPatientListHandler> logger) : IRequestHandler<GetPatientListQuery, Result<PagedDTO<PatientListDTO>>>
    {
        public async Task<Result<PagedDTO<PatientListDTO>>> Handle(GetPatientListQuery request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Retrieving patient list");

            var filteredPatients = await repository.GetFiltered(request, cancellationToken);
            var totalPatients = await db.Patients.CountAsync(cancellationToken);

            var filteredPatientsDTO = filteredPatients.Select(patient => patient.ADto()).ToList(); ;

            var patientsDTO = new PagedDTO<PatientListDTO>
            {
                Elementos = filteredPatientsDTO,
                Total = totalPatients
            };

            logger.LogInformation("Patient list retrieved successfully with {PatientCount} patients", patientsDTO.Elementos.Count);

            return Result.Success(patientsDTO);
        }
    }
}
