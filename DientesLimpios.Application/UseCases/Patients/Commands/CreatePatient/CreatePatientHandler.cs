using DientesLimpios.Application.Interfaces.Persistence;
using DientesLimpios.Application.Utilities.Mediator;
using DientesLimpios.Domain.Common.ResultPattern;
using DientesLimpios.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace DientesLimpios.Application.UseCases.Patients.Commands.CreatePatient
{
    public class CreatePatientHandler(IApplicationDbContext db, ILogger<CreatePatientHandler> logger) : IRequestHandler<CreatePatientCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(CreatePatientCommand request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Creating patient with Name {Name} and Email {Email}", request.Name, request.Email);

            var patientResult = Patient.Create(request.Name, request.Email);

            if (patientResult.IsFailure)
                return Result.Failure<Guid>(patientResult.Error);

            var patient = patientResult.Value;

            db.Patients.Add(patient);
            await db.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Patient created successfully with ID: {PatientId}", patient.Id);

            return Result.Success(patient.Id);

        }
    }
}
