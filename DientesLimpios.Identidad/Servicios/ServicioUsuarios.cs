using System.Security.Claims;
using DientesLimpios.Aplicacion.Interfaces.Identidad;
using Microsoft.AspNetCore.Http;

namespace DientesLimpios.Identidad.Servicios
{
    public class ServicioUsuarios(IHttpContextAccessor httpContextAccessor) : IServicioUsuarios
    {
        public string ObtenerUsuarioId()
        {
            return httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier)!;
        }
    }
}
