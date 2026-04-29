using DientesLimpios.Aplicacion.Interfaces.Notificaciones;
using DientesLimpios.Dominio.Entidades;

namespace DientesLimpios.Aplicacion.CasosdeUso.Citas.Comandos.CrearCita
{
    public static class MapeadorExtensions
    {
        public static ConfirmacionCitaDTO ADto(this Cita cita)
        {
            return new ConfirmacionCitaDTO
            {
                Id = cita.Id,
                Fecha = cita.IntervaloDeTiempo.Inicio,
                Paciente = cita.Paciente!.Nombre,
                Paciente_Email = cita.Paciente.Email.Valor,
                Consultorio = cita.Consultorio!.Nombre,
                Dentista = cita.Dentista!.Nombre
            };
        }

    }
}
