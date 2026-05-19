using DientesLimpios.Application.Interfaces.Persistence;
using DientesLimpios.Application.Utilities.Mediator;
using DientesLimpios.Domain.Common.ResultPattern;
using DientesLimpios.Domain.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DientesLimpios.Application.UseCases.Dentists.Commands.UpdateDentist
{
    public class UpdateDentistHandler(IApplicationDbContext db, ILogger<UpdateDentistHandler> logger) : IRequestHandler<UpdateDentistCommand, Result>
    {
        public async Task<Result> Handle(UpdateDentistCommand request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Updating dentist with ID: {DentistId}", request.Id);

            var dentist = await db.Dentists.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (dentist is null)
                return Result.Failure(DomainErrors.Dentist.NotFound);

            var actualizarNombreResult = dentist.UpdateName(request.Name);
            if (actualizarNombreResult.IsFailure)
                return actualizarNombreResult;

            var actualizarEmailResult = dentist.UpdateEmail(request.Email);
            if (actualizarEmailResult.IsFailure)
                return actualizarEmailResult;

            await db.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Dentist updated successfully with ID: {DentistId}", request.Id);

            return Result.Success();
        }
    }
}
