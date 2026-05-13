using System.Security.Claims;
using DientesLimpios.Application.Interfaces.Identity;
using Microsoft.AspNetCore.Http;

namespace DientesLimpios.Identity.Services
{
    public class UserService(IHttpContextAccessor httpContextAccessor) : IUserService
    {
        public string GetUserId()
        {
            return httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier)!;
        }
    }
}
