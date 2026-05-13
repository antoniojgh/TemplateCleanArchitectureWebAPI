using System.Globalization;
using System.Net;
using System.Net.Mail;
using DientesLimpios.Application.Interfaces.Notifications;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DientesLimpios.Infrastructure.Notifications
{
    public class EmailService(IConfiguration configuration, ILogger<EmailService> logger) : INotificationService
    {
        public async Task SendAppointmentConfirmation(AppointmentConfirmationDTO appointment)
        {
            var subject = "Confirmación de appointment - Dientes Limpios";

            var body = $"""
        Estimado (a) {appointment.Patient}, 
            
        Su appointment con el Dr (Dra.) {appointment.Dentist} ha sido programada para el {appointment.Fecha.ToString("f", new CultureInfo("es-DO"))} en el office {appointment.Office}.

        ¡Le esperamos!

        Equipo de Dientes Limpios
        """;

            await SendMessage(appointment.Patient_Email, subject, body);

        }

        public async Task SendAppointmentReminder(AppointmentReminderDTO appointment)
        {
            var subject = "RECORDATORIO: Confirmación de appointment - Dientes Limpios";

            var body = $"""
            Estimado (a) {appointment.Patient}, 
            
            Le recordamos que tiene appointment con el Dr (Dra.) {appointment.Dentist} para el {appointment.Fecha.ToString("f", new CultureInfo("es-DO"))} en el office {appointment.Office}.

            ¡Le esperamos!

            Equipo de Dientes Limpios
            """;

            await SendMessage(appointment.Patient_Email, subject, body);

        }

        private async Task SendMessage(string recipientEmail, string subject, string body)
        {
            logger.LogInformation("Preparing to send email to {Recipient}. Subject: {Subject}", recipientEmail, subject);

            try
            {
                var ourEmail = configuration.GetValue<string>("CONFIGURACIONES_EMAIL:EMAIL");
                var password = configuration.GetValue<string>("CONFIGURACIONES_EMAIL:PASSWORD");
                var host = configuration.GetValue<string>("CONFIGURACIONES_EMAIL:HOST");
                var port = configuration.GetValue<int>("CONFIGURACIONES_EMAIL:PUERTO");

                var smtpClient = new SmtpClient(host, port);
                smtpClient.EnableSsl = true;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new NetworkCredential(ourEmail, password);

                var mensaje = new MailMessage(ourEmail!, recipientEmail, subject, body);
                await smtpClient.SendMailAsync(mensaje);

                logger.LogInformation("Email sent successfully to {Recipient}", recipientEmail);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "SMTP Error: Failed to send email to {Recipient}", recipientEmail);

                // Rethrow so the Application layer knows it failed
                throw;
            }
        }
    }
}
