using DientesLimpios.Domain.Entities;

namespace DientesLimpios.Application.UseCases.Offices.Queries.GetOfficeList
{
    public static class MapperExtensions
    {
        public static OfficeListDTO ADto(this Office office)
        {
            var dto = new OfficeListDTO { Id = office.Id, Name = office.Name };
            return dto;
        }
    }
}
