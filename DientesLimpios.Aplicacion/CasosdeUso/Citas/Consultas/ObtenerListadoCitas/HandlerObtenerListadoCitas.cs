using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using DientesLimpios.Aplicacion.Interfaces.Repositorios;
using MediatR;

namespace DientesLimpios.Aplicacion.CasosdeUso.Citas.Consultas.ObtenerListadoCitas
{
    public class HandlerObtenerListadoCitas(IRepositorioCitas repositorio, IMapper mapper) : IRequestHandler<ConsultaObtenerListadoCitas, List<CitaListadoDTO>>
    {
        public async Task<List<CitaListadoDTO>> Handle(ConsultaObtenerListadoCitas request, CancellationToken cancellationToken)
        {
            var citas = await repositorio.ObtenerFiltrado(request);

            var citasDTO = mapper.Map<List<CitaListadoDTO>>(citas);

            return citasDTO;
        }
    }
}
