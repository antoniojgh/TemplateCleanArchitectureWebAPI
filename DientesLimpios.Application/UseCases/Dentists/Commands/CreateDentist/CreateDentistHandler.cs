using DientesLimpios.Application.Interfaces.Persistence;
using DientesLimpios.Application.Interfaces.Repositories;
using DientesLimpios.Application.Utilities.Mediator;
using DientesLimpios.Domain.Common.ResultPattern;
using DientesLimpios.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace DientesLimpios.Application.UseCases.Dentists.Commands.CreateDentist
{
    public class CreateDentistHandler(IDentistRepository repository, IUnitOfWork unitOfWork, ILogger<CreateDentistHandler> logger) : IRequestHandler<CreateDentistCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(CreateDentistCommand request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Creating dentist with Name {Name} and Email {Email}", request.Name, request.Email);

            var dentistResult = Dentist.Create(request.Name, request.Email);

            if (dentistResult.IsFailure)
                return Result.Failure<Guid>(dentistResult.Error);

            var dentist = dentistResult.Value;

            await repository.Add(dentist);
            await unitOfWork.SaveChanges();

            logger.LogInformation("Dentist created successfully with ID: {DentistId}", dentist.Id);

            return Result.Success(dentist.Id);
        }
    }
}
