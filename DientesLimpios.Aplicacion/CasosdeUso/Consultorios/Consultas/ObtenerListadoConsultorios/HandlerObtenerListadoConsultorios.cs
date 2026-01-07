using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using DientesLimpios.Aplicacion.CasosdeUso.Consultorios.Consultas.ObtenerDetalleConsultorio;
using DientesLimpios.Aplicacion.Interfaces.Repositorios;
using DientesLimpios.Dominio.Entidades;
using MediatR;

namespace DientesLimpios.Aplicacion.CasosdeUso.Consultorios.Consultas.ObtenerListadoConsultorios
{
    public class HandlerObtenerListadoConsultorios(IRepositorioConsultorios repositorio, IMapper mapper) : IRequestHandler<ConsultaObtenerListadoConsultorios, List<ConsultorioListadoDTO>>
    {
        public async Task<List<ConsultorioListadoDTO>> Handle(ConsultaObtenerListadoConsultorios request, CancellationToken cancellationToken)
        {
            var consultorios = await repositorio.ObtenerTodos();
            var consultoriosDTO = mapper.Map<List<ConsultorioListadoDTO>>(consultorios);
            return consultoriosDTO;
        }
    }
}
