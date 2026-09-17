using FluentValidation;

namespace DientesLimpios.Application.UseCases.Appointments.Commands.CreateAppointment
{
    public class CreateAppointmentCommandValidator : AbstractValidator<CreateAppointmentCommand>
    {
        public CreateAppointmentCommandValidator(TimeProvider timeProvider)
        {
            // The lambda runs on every Validate call, so "now" is the moment of validation,
            // not the moment this validator was constructed.
            RuleFor(x => x.StartDate)
                .LessThan(x => x.EndDate).WithMessage("The start date must be earlier than the end date")
                .GreaterThan(_ => timeProvider.GetUtcNow().UtcDateTime).WithMessage("The start date cannot be in the past");

        }
    }
}
