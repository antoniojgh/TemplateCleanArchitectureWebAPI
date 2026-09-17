using FluentValidation;

namespace DientesLimpios.Application.UseCases.Appointments.Queries.GetAppointmentList
{
    public class GetAppointmentListQueryValidator : AbstractValidator<GetAppointmentListQuery>
    {
        public GetAppointmentListQueryValidator()
        {
            RuleFor(x => x.StartDate)
                .Must(BeUtc).WithMessage("The {PropertyName} filter must include an offset or 'Z'");

            RuleFor(x => x.EndDate)
                .Must(BeUtc).WithMessage("The {PropertyName} filter must include an offset or 'Z'");
        }

        // The model binder turns offset and "Z" values into UTC; anything else is ambiguous.
        private static bool BeUtc(DateTime? value) => value is null || value.Value.Kind == DateTimeKind.Utc;
    }
}
