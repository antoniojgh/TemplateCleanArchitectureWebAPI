using FluentValidation;

namespace DientesLimpios.Application.UseCases.Patients.Queries.GetPatientList
{
    public sealed class GetPatientListQueryValidator : AbstractValidator<GetPatientListQuery>
    {
        public const int MaxRecordsPerPage = 100;

        public GetPatientListQueryValidator()
        {
            RuleFor(x => x.Page)
                .GreaterThanOrEqualTo(1);

            RuleFor(x => x.RecordsPerPage)
                .InclusiveBetween(1, MaxRecordsPerPage);

            RuleFor(x => x.Name)
                .MaximumLength(100)
                .When(x => x.Name is not null);

            RuleFor(x => x.Email)
                .MaximumLength(254)
                .When(x => x.Email is not null);
        }
    }
}

