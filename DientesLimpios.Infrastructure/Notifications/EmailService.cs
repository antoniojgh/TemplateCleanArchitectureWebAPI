using System.Net;
using System.Net.Mail;
using DientesLimpios.Application.Configuration;
using DientesLimpios.Application.Interfaces.Notifications;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DientesLimpios.Infrastructure.Notifications
{
    public class EmailService(IOptions<EmailOptions> options, IOptions<ClinicOptions> clinicOptions, ILogger<EmailService> logger) : INotificationService
    {
        private readonly EmailOptions _options = options.Value;
        private readonly TimeZoneInfo _clinicTimeZone = TimeZoneInfo.FindSystemTimeZoneById(clinicOptions.Value.TimeZoneId);
        public async Task SendAppointmentConfirmation(AppointmentConfirmationDTO appointment, CancellationToken cancellationToken)
        {
            var subject = "Appointment Confirmation - Dientes Limpios";

            var body = $"""
            Dear {appointment.Patient},

            Your appointment with Dr. {appointment.Dentist} has been scheduled for {AppointmentDateFormatter.ToClinicTime(appointment.Date, _clinicTimeZone)} at the {appointment.Office} office.

            We look forward to seeing you!

            The Dientes Limpios Team
            """;

            await SendMessage(appointment.PatientEmail, subject, body, cancellationToken);
        }

        public async Task SendAppointmentReminder(AppointmentReminderDTO appointment, CancellationToken cancellationToken)
        {
            var subject = "REMINDER: Appointment Confirmation - Dientes Limpios";

            var body = $"""
            Dear {appointment.Patient},

            This is a reminder that you have an appointment with Dr. {appointment.Dentist} on {AppointmentDateFormatter.ToClinicTime(appointment.Date, _clinicTimeZone)} at the {appointment.Office} office.

            We look forward to seeing you!

            The Dientes Limpios Team
            """;

            await SendMessage(appointment.PatientEmail, subject, body, cancellationToken);
        }

        private async Task SendMessage(string recipientEmail, string subject, string body, CancellationToken cancellationToken)
        {
            logger.LogInformation("Preparing to send email to {Recipient}. Subject: {Subject}", recipientEmail, subject);

            try
            {
                using var smtpClient = new SmtpClient(_options.Host, _options.Port)
                {
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(_options.Email, _options.Password),
                };

                using var message = new MailMessage(_options.Email, recipientEmail, subject, body);
                await smtpClient.SendMailAsync(message, cancellationToken);

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