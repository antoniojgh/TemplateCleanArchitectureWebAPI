using DientesLimpios.Application.Interfaces.Repositories;
using DientesLimpios.Application.Utilities.Mediator;
using DientesLimpios.Domain.Common.ResultPattern;
using DientesLimpios.Domain.Entities;
using DientesLimpios.Domain.Errors;
using DientesLimpios.Application.Interfaces.Persistence;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;


namespace DientesLimpios.Application.UseCases.Appointments.Commands.CreateAppointment
{
    public class CreateAppointmentHandler(IApplicationDbContext db, IAppointmentRepository repository, ILogger<CreateAppointmentHandler> logger) : IRequestHandler<CreateAppointmentCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(CreateAppointmentCommand request, CancellationToken cancellationToken)
        {
            logger.LogInformation(
            "Creating appointment for Patient {PatientId} with Dentist {DentistId} at Office {OfficeId}",
            request.PatientId, request.DentistId, request.OfficeId);

            // Validate that the patient exists before proceeding
            if (!await db.Patients.AnyAsync(p => p.Id == request.PatientId, cancellationToken))
                return Result.Failure<Guid>(DomainErrors.Patient.NotFound);

            // Validate that the dentist exists before proceeding
            if (!await db.Dentists.AnyAsync(d => d.Id == request.DentistId, cancellationToken))
                return Result.Failure<Guid>(DomainErrors.Dentist.NotFound);

            // Validate that the office exists before proceeding
            if (!await db.Offices.AnyAsync(o => o.Id == request.OfficeId, cancellationToken))
                return Result.Failure<Guid>(DomainErrors.Office.NotFound);

            // The overlap check, the construction of the aggregate and the insert all happen
            // inside one transaction, serialised per dentist.
            var addResult = await repository.AddIfNoOverlap(request.PatientId, request.DentistId, request.OfficeId, request.StartDate, request.EndDate, cancellationToken);

            if (addResult.IsFailure)
                return Result.Failure<Guid>(addResult.Error);

            logger.LogInformation("Appointment created successfully with ID: {AppointmentId}", addResult.Value);

            return Result.Success(addResult.Value);
        }
    }
}
