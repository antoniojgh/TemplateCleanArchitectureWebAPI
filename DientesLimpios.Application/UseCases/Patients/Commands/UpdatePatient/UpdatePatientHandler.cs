using DientesLimpios.Application.Interfaces.Persistence;
using DientesLimpios.Application.Utilities.Mediator;
using DientesLimpios.Domain.Common.ResultPattern;
using DientesLimpios.Domain.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DientesLimpios.Application.UseCases.Patients.Commands.UpdatePatient
{
    public class UpdatePatientHandler(IApplicationDbContext db, ILogger<UpdatePatientHandler> logger) : IRequestHandler<UpdatePatientCommand, Result>
    {
        public async Task<Result> Handle(UpdatePatientCommand request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Updating patient with ID: {PatientId}", request.Id);

            var patient = await db.Patients.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (patient is null)
                return Result.Failure(DomainErrors.Patient.NotFound);

            var actualizarNombreResult = patient.UpdateName(request.Name);
            if (actualizarNombreResult.IsFailure)
                return actualizarNombreResult;

            var actualizarEmailResult = patient.UpdateEmail(request.Email);
            if (actualizarEmailResult.IsFailure)
                return actualizarEmailResult;

            await db.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Patient updated successfully with ID: {PatientId}", request.Id);

            return Result.Success();

        }
    }
}
