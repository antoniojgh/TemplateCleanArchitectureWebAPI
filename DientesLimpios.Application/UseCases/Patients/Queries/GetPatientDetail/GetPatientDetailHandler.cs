using DientesLimpios.Application.Interfaces.Persistence;
using DientesLimpios.Application.Utilities.Mediator;
using DientesLimpios.Domain.Common.ResultPattern;
using DientesLimpios.Domain.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DientesLimpios.Application.UseCases.Patients.Queries.GetPatientDetail
{
    public class GetPatientDetailHandler(IApplicationDbContext db, ILogger<GetPatientDetailHandler> logger) : IRequestHandler<GetPatientDetailQuery, Result<PatientDetailDTO>>
    {
        public async Task<Result<PatientDetailDTO>> Handle(GetPatientDetailQuery request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Retrieving patient detail with ID: {PatientId}", request.Id);

            var patient = await db.Patients
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);


            if (patient is null)
                return Result.Failure<PatientDetailDTO>(DomainErrors.Patient.NotFound);

            logger.LogInformation("Patient detail retrieved successfully with ID: {PatientId}", request.Id);

            return Result.Success(patient.ADto());
        }
    }
}
