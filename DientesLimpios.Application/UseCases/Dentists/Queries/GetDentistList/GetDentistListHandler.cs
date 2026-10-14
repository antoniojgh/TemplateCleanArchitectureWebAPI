using DientesLimpios.Application.Interfaces.Persistence;
using DientesLimpios.Application.Utilities.Common;
using DientesLimpios.Application.Utilities.Mediator;
using DientesLimpios.Domain.Common.ResultPattern;
using Microsoft.Extensions.Logging;


namespace DientesLimpios.Application.UseCases.Dentists.Queries.GetDentistList
{
    public class GetDentistListHandler(IApplicationDbContext db, ILogger<GetDentistListHandler> logger) : IRequestHandler<GetDentistListQuery, Result<PagedDTO<DentistListDTO>>>
    {
        public async Task<Result<PagedDTO<DentistListDTO>>> Handle(GetDentistListQuery request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Retrieving dentist list");

            var dentists = db.Dentists.AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Name))
                dentists = dentists.Where(d => d.Name.Contains(request.Name));

            if (!string.IsNullOrWhiteSpace(request.Email))
                dentists = dentists.Where(d => d.Email.Value.Contains(request.Email));

            var page = await dentists
                .OrderBy(d => d.Name)
                .ThenBy(d => d.Id)          // tie-breaker: stable pages when two dentists share a name
                .Select(d => new DentistListDTO { Id = d.Id, Name = d.Name, Email = d.Email.Value })
                .ToPagedAsync(request.Page, request.RecordsPerPage, cancellationToken);

            logger.LogInformation("Dentist list retrieved successfully with {DentistCount} dentists", page.Items.Count);

            return Result.Success(page);
        }
    }
}
