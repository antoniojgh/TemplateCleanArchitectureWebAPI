using FluentValidation;

namespace DientesLimpios.Application.UseCases.Appointments.Commands.RescheduleAppointment
{
    public class RescheduleAppointmentCommandValidator : AbstractValidator<RescheduleAppointmentCommand>
    {
        public RescheduleAppointmentCommandValidator(TimeProvider timeProvider)
        {
            // The lambda runs on every Validate call, so "now" is the moment of validation,
            // not the moment this validator was constructed.
            RuleFor(x => x.StartDate)
                .LessThan(x => x.EndDate).WithMessage("The start date must be earlier than the end date")
                .GreaterThan(_ => timeProvider.GetUtcNow().UtcDateTime).WithMessage("The start date cannot be in the past");
        }
    }
}
