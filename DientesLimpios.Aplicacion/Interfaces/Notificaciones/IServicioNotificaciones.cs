namespace DientesLimpios.Aplicacion.Interfaces.Notificaciones
{
    public interface IServicioNotificaciones
    {
        Task EnviarConfirmacionCita(ConfirmacionCitaDTO cita);
        Task EnviarRecordatorioCita(RecordatorioCitaDTO cita);
    }
}
