using System.Globalization;
using System.Net;
using System.Net.Mail;
using DientesLimpios.Aplicacion.Interfaces.Notificaciones;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DientesLimpios.Infraestructura.Notificaciones
{
    public class ServicioCorreos(IConfiguration configuration, ILogger<ServicioCorreos> logger) : IServicioNotificaciones
    {
        public async Task EnviarConfirmacionCita(ConfirmacionCitaDTO cita)
        {
            var asunto = "Confirmación de cita - Dientes Limpios";

            var cuerpo = $"""
        Estimado (a) {cita.Paciente}, 
            
        Su cita con el Dr (Dra.) {cita.Dentista} ha sido programada para el {cita.Fecha.ToString("f", new CultureInfo("es-DO"))} en el consultorio {cita.Consultorio}.

        ¡Le esperamos!

        Equipo de Dientes Limpios
        """;

            await EnviarMensaje(cita.Paciente_Email, asunto, cuerpo);

        }

        public async Task EnviarRecordatorioCita(RecordatorioCitaDTO cita)
        {
            var asunto = "RECORDATORIO: Confirmación de cita - Dientes Limpios";

            var cuerpo = $"""
            Estimado (a) {cita.Paciente}, 
            
            Le recordamos que tiene cita con el Dr (Dra.) {cita.Dentista} para el {cita.Fecha.ToString("f", new CultureInfo("es-DO"))} en el consultorio {cita.Consultorio}.

            ¡Le esperamos!

            Equipo de Dientes Limpios
            """;

            await EnviarMensaje(cita.Paciente_Email, asunto, cuerpo);

        }

        private async Task EnviarMensaje(string emailDestinatario, string asunto, string cuerpo)
        {
            logger.LogInformation("Preparing to send email to {Recipient}. Subject: {Subject}", emailDestinatario, asunto);

            try
            {
                var nuestroEmail = configuration.GetValue<string>("CONFIGURACIONES_EMAIL:EMAIL");
                var password = configuration.GetValue<string>("CONFIGURACIONES_EMAIL:PASSWORD");
                var host = configuration.GetValue<string>("CONFIGURACIONES_EMAIL:HOST");
                var puerto = configuration.GetValue<int>("CONFIGURACIONES_EMAIL:PUERTO");

                var smtpCliente = new SmtpClient(host, puerto);
                smtpCliente.EnableSsl = true;
                smtpCliente.UseDefaultCredentials = false;
                smtpCliente.Credentials = new NetworkCredential(nuestroEmail, password);

                var mensaje = new MailMessage(nuestroEmail!, emailDestinatario, asunto, cuerpo);
                await smtpCliente.SendMailAsync(mensaje);

                logger.LogInformation("Email sent successfully to {Recipient}", emailDestinatario);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "SMTP Error: Failed to send email to {Recipient}", emailDestinatario);

                // Rethrow so the Application layer knows it failed
                throw;
            }
        }
    }
}
