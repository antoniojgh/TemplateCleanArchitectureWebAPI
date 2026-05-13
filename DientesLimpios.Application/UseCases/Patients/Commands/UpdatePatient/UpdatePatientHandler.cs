using DientesLimpios.Application.Interfaces.Persistence;
using DientesLimpios.Application.Interfaces.Repositories;
using DientesLimpios.Application.Utilities.Mediator;
using DientesLimpios.Domain.Common.ResultPattern;
using DientesLimpios.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace DientesLimpios.Application.UseCases.Patients.Commands.UpdatePatient
{
    public class UpdatePatientHandler(IPatientRepository repository, IUnitOfWork unitOfWork, ILogger<UpdatePatientHandler> logger) : IRequestHandler<UpdatePatientCommand, Result>
    {
        public async Task<Result> Handle(UpdatePatientCommand request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Actualizando patient con ID: {PatientId}", request.Id);

            var patient = await repository.GetById(request.Id);

            if (patient is null)
                return Result.Failure(DomainErrors.Patient.NotFound);

            var actualizarNombreResult = patient.UpdateName(request.Name);
            if (actualizarNombreResult.IsFailure)
                return actualizarNombreResult;

            var actualizarEmailResult = patient.UpdateEmail(request.Email);
            if (actualizarEmailResult.IsFailure)
                return actualizarEmailResult;

            await repository.Update(patient);
            await unitOfWork.SaveChanges();

            logger.LogInformation("Patient actualizado correctamente con ID: {PatientId}", request.Id);

            return Result.Success();

        }
    }
}
