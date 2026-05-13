using DientesLimpios.Application.Utilities.Mediator;
using DientesLimpios.Domain.Common.ResultPattern;

namespace DientesLimpios.Application.UseCases.Appointments.Queries.GetAppointmentDetail
{
    public class GetAppointmentDetailQuery : IRequest<Result<AppointmentDetailDTO>>
    {
        public required Guid Id { get; set; }
    }
}
