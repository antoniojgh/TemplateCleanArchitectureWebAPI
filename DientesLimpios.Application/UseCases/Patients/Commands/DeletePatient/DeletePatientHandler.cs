using DientesLimpios.Application.Interfaces.Persistence;
using DientesLimpios.Application.Utilities.Mediator;
using DientesLimpios.Domain.Common.ResultPattern;
using DientesLimpios.Domain.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DientesLimpios.Application.UseCases.Patients.Commands.DeletePatient
{
    public class DeletePatientHandler(IApplicationDbContext db, ILogger<DeletePatientHandler> logger) : IRequestHandler<DeletePatientCommand, Result>
    {
        public async Task<Result> Handle(DeletePatientCommand request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Deleting patient with ID: {PatientId}", request.Id);

            var patient = await db.Patients.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (patient is null)
                return Result.Failure(DomainErrors.Patient.NotFound);

            db.Patients.Remove(patient);
            await db.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Patient deleted successfully with ID: {PatientId}", request.Id);

            return Result.Success();
        }
    }
}
