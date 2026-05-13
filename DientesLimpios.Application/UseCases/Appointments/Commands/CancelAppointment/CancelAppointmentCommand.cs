using DientesLimpios.Application.Utilities.Mediator;
using DientesLimpios.Domain.Common.ResultPattern;

namespace DientesLimpios.Application.UseCases.Appointments.Commands.CancelAppointment
{
    public class CancelAppointmentCommand : IRequest<Result>
    {
        public required Guid Id { get; set; }
    }
}
