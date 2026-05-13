using DientesLimpios.Application.Utilities.Mediator;
using DientesLimpios.Domain.Common.ResultPattern;

namespace DientesLimpios.Application.UseCases.Appointments.Commands.CreateAppointment
{
    public class CreateAppointmentCommand : IRequest<Result<Guid>>
    {
        public Guid PatientId { get; set; }
        public Guid DentistId { get; set; }
        public Guid OfficeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
