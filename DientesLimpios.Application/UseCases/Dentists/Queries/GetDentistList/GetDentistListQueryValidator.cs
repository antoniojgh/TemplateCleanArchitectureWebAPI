using FluentValidation;
using DientesLimpios.Domain.Entities;
using DientesLimpios.Domain.ValueObjects;


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
                .MaximumLength(Dentist.NameMaxLength)
                .When(x => x.Name is not null);

            RuleFor(x => x.Email)
                .MaximumLength(Email.MaxLength)
                .When(x => x.Email is not null);
        }
    }
}