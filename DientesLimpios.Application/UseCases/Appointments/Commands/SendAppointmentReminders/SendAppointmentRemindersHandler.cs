using DientesLimpios.Application.UseCases.Appointments.Commands.CancelAppointment;
using DientesLimpios.Application.Interfaces.Notifications;
using DientesLimpios.Application.Interfaces.Repositories;
using DientesLimpios.Application.Interfaces.Repositories.Models;
using DientesLimpios.Application.Utilities.Mediator;
using DientesLimpios.Domain.Common.ResultPattern;
using DientesLimpios.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace DientesLimpios.Application.UseCases.Appointments.Commands.SendAppointmentReminders
{
    public class SendAppointmentRemindersHandler(IAppointmentRepository repository,
                INotificationService notificationService, ILogger<SendAppointmentRemindersHandler> logger) : IRequestHandler<SendAppointmentRemindersCommand, Result>
    {
        public async Task<Result> Handle(SendAppointmentRemindersCommand request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Enviando recordatorio de appointments para el día: {Fecha}", DateTime.UtcNow.Date.AddDays(1));

            var mañana = DateTime.UtcNow.Date.AddDays(1);
            var startDate = mañana;
            var endDate = mañana.AddDays(1);

            var filter = new AppointmentFilterDTO
            {
                StartDate = startDate,
                EndDate = endDate,
                AppointmentStatus = AppointmentStatus.Scheduled
            };

            var appointments = await repository.GetFiltered(filter);

            foreach (var appointment in appointments)
            {
                var appointmentDTO = appointment.ADto();
                await notificationService.SendAppointmentReminder(appointmentDTO);
            }

            logger.LogInformation("Reminder de appointments enviado correctamente para {NumeroAppointments} appointments", appointments.Count());

            return Result.Success();
        }
    }
}
