using DientesLimpios.Domain.Entities;

namespace DientesLimpios.Application.UseCases.Dentists.Queries.GetDentistList
{
    public static class MapperExtensions
    {
        public static DentistListDTO ADto(this Dentist Dentist)
        {
            var dto = new DentistListDTO { Id = Dentist.Id, Name = Dentist.Name, Email = Dentist.Email.Value };
            return dto;
        }
    }
}
