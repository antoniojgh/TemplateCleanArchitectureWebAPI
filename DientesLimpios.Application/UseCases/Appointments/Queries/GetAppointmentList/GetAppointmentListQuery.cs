using DientesLimpios.Application.Interfaces.Repositories.Models;
using DientesLimpios.Application.Utilities.Mediator;
using DientesLimpios.Domain.Common.ResultPattern;

namespace DientesLimpios.Application.UseCases.Appointments.Queries.GetAppointmentList
{
    public class GetAppointmentListQuery : AppointmentFilterDTO, IRequest<Result<List<AppointmentListDTO>>>
    {
    }
}
