using DientesLimpios.Application.Interfaces.Persistence;
using DientesLimpios.Application.Interfaces.Repositories;
using DientesLimpios.Application.Utilities.Mediator;
using DientesLimpios.Domain.Common.ResultPattern;
using DientesLimpios.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace DientesLimpios.Application.UseCases.Patients.Commands.DeletePatient
{
    public class DeletePatientHandler(IPatientRepository repository, IUnitOfWork unitOfWork, ILogger<DeletePatientHandler> logger) : IRequestHandler<DeletePatientCommand, Result>
    {
        public async Task<Result> Handle(DeletePatientCommand request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Borrando patient con ID: {PatientId}", request.Id);

            var patient = await repository.GetById(request.Id);

            if (patient is null)
                return Result.Failure(DomainErrors.Patient.NotFound);

            await repository.Delete(patient);
            await unitOfWork.SaveChanges();

            logger.LogInformation("Patient borrado correctamente con ID: {PatientId}", request.Id);

            return Result.Success();
        }
    }
}
