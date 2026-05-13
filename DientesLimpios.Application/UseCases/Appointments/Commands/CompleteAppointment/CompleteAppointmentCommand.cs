using DientesLimpios.Application.Utilities.Mediator;
using DientesLimpios.Domain.Common.ResultPattern;

namespace DientesLimpios.Application.UseCases.Appointments.Commands.CompleteAppointment
{
    public class CompleteAppointmentCommand : IRequest<Result>
    {
        public required Guid Id { get; set; }
    }
}
