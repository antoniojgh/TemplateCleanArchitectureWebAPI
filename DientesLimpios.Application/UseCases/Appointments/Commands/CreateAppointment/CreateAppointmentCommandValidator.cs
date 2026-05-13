using FluentValidation;

namespace DientesLimpios.Application.UseCases.Appointments.Commands.CreateAppointment
{
    public class CreateAppointmentCommandValidator : AbstractValidator<CreateAppointmentCommand>
    {
        public CreateAppointmentCommandValidator()
        {
            RuleFor(x => x.StartDate)
                .LessThan(x => x.EndDate).WithMessage("La fecha de start debe ser anterior a la fecha de end")
                .GreaterThan(DateTime.UtcNow).WithMessage("La fecha start no puede estar en el pasado");

        }
    }
}
