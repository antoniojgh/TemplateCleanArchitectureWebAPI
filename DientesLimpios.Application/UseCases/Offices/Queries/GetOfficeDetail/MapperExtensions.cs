using DientesLimpios.Domain.Entities;

namespace DientesLimpios.Application.UseCases.Offices.Queries.GetOfficeDetail
{
    public static class MapperExtensions
    {
        public static OfficeDetailDTO ADto(this Office office)
        {
            var dto = new OfficeDetailDTO { Id = office.Id, Name = office.Name };
            return dto;
        }
    }
}
