using FluentValidation;


namespace DientesLimpios.Application.UseCases.Dentists.Queries.GetDentistList
{
    public sealed class GetDentistListQueryValidator : AbstractValidator<GetDentistListQuery>
    {
        public const int MaxRecordsPerPage = 100;

        public GetDentistListQueryValidator()
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