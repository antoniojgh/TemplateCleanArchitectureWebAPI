using DientesLimpios.Application.Interfaces.Persistence;
using DientesLimpios.Application.Utilities.Mediator;
using DientesLimpios.Domain.Common.ResultPattern;
using DientesLimpios.Domain.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DientesLimpios.Application.UseCases.Dentists.Commands.DeleteDentist
{
    public class DeleteDentistHandler(IApplicationDbContext db, ILogger<DeleteDentistHandler> logger) : IRequestHandler<DeleteDentistCommand, Result>
    {
        public async Task<Result> Handle(DeleteDentistCommand request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Deleting dentist with ID: {DentistId}", request.Id);

            var dentist = await db.Dentists.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (dentist is null)
                return Result.Failure(DomainErrors.Dentist.NotFound);

            // Appointments are history. Refuse explicitly rather than let the foreign key throw,
            // so the caller gets a 409 with a domain error instead of a 500.
            var hasAppointments = await db.Appointments.AnyAsync(a => a.DentistId == request.Id, cancellationToken);

            if (hasAppointments)
                return Result.Failure(DomainErrors.Dentist.HasAppointmentsConflict);

            db.Dentists.Remove(dentist);
            await db.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Dentist deleted successfully with ID: {DentistId}", request.Id);

            return Result.Success();
        }
    }
}
