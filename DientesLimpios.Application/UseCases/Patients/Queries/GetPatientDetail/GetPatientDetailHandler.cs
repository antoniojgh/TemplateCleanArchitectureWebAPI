using DientesLimpios.Application.Interfaces.Repositories;
using DientesLimpios.Application.Utilities.Mediator;
using DientesLimpios.Domain.Common.ResultPattern;
using DientesLimpios.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace DientesLimpios.Application.UseCases.Patients.Queries.GetPatientDetail
{
    public class GetPatientDetailHandler(IPatientRepository repository, ILogger<GetPatientDetailHandler> logger) : IRequestHandler<GetPatientDetailQuery, Result<PatientDetailDTO>>
    {
        public async Task<Result<PatientDetailDTO>> Handle(GetPatientDetailQuery request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Retrieving patient detail with ID: {PatientId}", request.Id);

            var patient = await repository.GetById(request.Id);

            if (patient is null)
                return Result.Failure<PatientDetailDTO>(DomainErrors.Patient.NotFound);

            logger.LogInformation("Patient detail retrieved successfully with ID: {PatientId}", request.Id);

            return Result.Success(patient.ADto());
        }
    }
}
