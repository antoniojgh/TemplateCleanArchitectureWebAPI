using DientesLimpios.Domain.Entities;

namespace DientesLimpios.Application.UseCases.Dentists.Queries.GetDentistDetail
{
    public static class MapperExtensions
    {
        public static DentistDetailDTO ADto(this Dentist dentist)
        {
            var dto = new DentistDetailDTO { Id = dentist.Id, Name = dentist.Name, Email = dentist.Email.Value };
            return dto;
        }
    }
}
