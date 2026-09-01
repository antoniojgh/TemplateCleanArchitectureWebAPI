using FluentValidation;

namespace DientesLimpios.Application.UseCases.Appointments.Commands.CreateAppointment
{
    public class CreateAppointmentCommandValidator : AbstractValidator<CreateAppointmentCommand>
    {
        public CreateAppointmentCommandValidator()
        {
            RuleFor(x => x.StartDate)
                .LessThan(x => x.EndDate).WithMessage("The start date must be earlier than the end date")
                .GreaterThan(DateTime.UtcNow).WithMessage("The start date cannot be in the past");

        }
    }
}
