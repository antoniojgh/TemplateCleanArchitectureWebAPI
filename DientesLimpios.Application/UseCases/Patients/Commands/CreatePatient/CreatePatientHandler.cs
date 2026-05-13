using DientesLimpios.Application.Interfaces.Persistence;
using DientesLimpios.Application.Interfaces.Repositories;
using DientesLimpios.Application.Utilities.Mediator;
using DientesLimpios.Domain.Common.ResultPattern;
using DientesLimpios.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace DientesLimpios.Application.UseCases.Patients.Commands.CreatePatient
{
    public class CreatePatientHandler(IPatientRepository repository, IUnitOfWork unitOfWork, ILogger<CreatePatientHandler> logger) : IRequestHandler<CreatePatientCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(CreatePatientCommand request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Creando patient con Name {Name} y Email {Email}", request.Name, request.Email);

            var patientResult = Patient.Create(request.Name, request.Email);

            if (patientResult.IsFailure)
                return Result.Failure<Guid>(patientResult.Error);

            var patient = patientResult.Value;

            await repository.Add(patient);
            await unitOfWork.SaveChanges();

            logger.LogInformation("Patient creado correctamente con ID: {PatientId}", patient.Id);

            return Result.Success(patient.Id);

        }
    }
}
