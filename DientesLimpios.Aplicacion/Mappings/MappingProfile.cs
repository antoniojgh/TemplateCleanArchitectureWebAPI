using AutoMapper;
using DientesLimpios.Aplicacion.CasosdeUso.Citas.Consultas.ObtenerDetalleCita;
using DientesLimpios.Aplicacion.CasosdeUso.Citas.Consultas.ObtenerListadoCitas;
using DientesLimpios.Aplicacion.CasosdeUso.Consultorios.Consultas.ObtenerDetalleConsultorio;
using DientesLimpios.Aplicacion.CasosdeUso.Consultorios.Consultas.ObtenerListadoConsultorios;
using DientesLimpios.Aplicacion.CasosdeUso.Dentistas.Consultas.ObtenerDetalleDentista;
using DientesLimpios.Aplicacion.CasosdeUso.Dentistas.Consultas.ObtenerListadoDentistas;
using DientesLimpios.Aplicacion.CasosdeUso.Pacientes.Consultas.ObtenerDetallePaciente;
using DientesLimpios.Aplicacion.CasosdeUso.Pacientes.Consultas.ObtenerListadoPacientes;
using DientesLimpios.Aplicacion.Interfaces.Notificaciones;
using DientesLimpios.Dominio.Entidades;

namespace DientesLimpios.Aplicacion.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // CreateMap<Source, Destination>();
            // If property names match (Id -> Id, Nombre -> Nombre), 
            // AutoMapper does this automatically! No need to specify fields.


            CreateMap<Consultorio, ConsultorioDetalleDTO>();
            CreateMap<Consultorio, ConsultorioListadoDTO>();

            CreateMap<Paciente, PacienteListadoDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.Nombre))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Valor));

            CreateMap<Paciente, PacienteDetalleDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.Nombre))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Valor));

            CreateMap<Dentista, DentistaListadoDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.Nombre))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Valor));

            CreateMap<Dentista, DentistaDetalleDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.Nombre))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Valor));

            CreateMap<Cita, CitaDetalleDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.FechaInicio, opt => opt.MapFrom(src => src.IntervaloDeTiempo.Inicio))
                .ForMember(dest => dest.FechaFin, opt => opt.MapFrom(src => src.IntervaloDeTiempo.Fin))
                .ForMember(dest => dest.Consultorio, opt => opt.MapFrom(src => src.Consultorio!.Nombre))
                .ForMember(dest => dest.Dentista, opt => opt.MapFrom(src => src.Dentista!.Nombre))
                .ForMember(dest => dest.Paciente, opt => opt.MapFrom(src => src.Paciente!.Nombre))
                .ForMember(dest => dest.EstadoCita, opt => opt.MapFrom(src => src.Estado.ToString()));

            CreateMap<Cita, CitaListadoDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.FechaInicio, opt => opt.MapFrom(src => src.IntervaloDeTiempo.Inicio))
                .ForMember(dest => dest.FechaFin, opt => opt.MapFrom(src => src.IntervaloDeTiempo.Fin))
                .ForMember(dest => dest.Consultorio, opt => opt.MapFrom(src => src.Consultorio!.Nombre))
                .ForMember(dest => dest.Dentista, opt => opt.MapFrom(src => src.Dentista!.Nombre))
                .ForMember(dest => dest.Paciente, opt => opt.MapFrom(src => src.Paciente!.Nombre))
                .ForMember(dest => dest.EstadoCita, opt => opt.MapFrom(src => src.Estado.ToString()));

            CreateMap<Cita, ConfirmacionCitaDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Fecha, opt => opt.MapFrom(src => src.IntervaloDeTiempo.Inicio))
                .ForMember(dest => dest.Paciente, opt => opt.MapFrom(src => src.Paciente!.Nombre))
                .ForMember(dest => dest.Paciente_Email, opt => opt.MapFrom(src => src.Paciente!.Email.Valor))
                .ForMember(dest => dest.Consultorio, opt => opt.MapFrom(src => src.Consultorio!.Nombre))
                .ForMember(dest => dest.Dentista, opt => opt.MapFrom(src => src.Dentista!.Nombre));

            CreateMap<Cita, RecordatorioCitaDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Fecha, opt => opt.MapFrom(src => src.IntervaloDeTiempo.Inicio))
                .ForMember(dest => dest.Paciente, opt => opt.MapFrom(src => src.Paciente!.Nombre))
                .ForMember(dest => dest.Paciente_Email, opt => opt.MapFrom(src => src.Paciente!.Email.Valor))
                .ForMember(dest => dest.Consultorio, opt => opt.MapFrom(src => src.Consultorio!.Nombre))
                .ForMember(dest => dest.Dentista, opt => opt.MapFrom(src => src.Dentista!.Nombre));
        }
    }
}