using DientesLimpios.Domain.Entities;

namespace DientesLimpios.Application.UseCases.Patients.Queries.GetPatientDetail
{
    public static class MapperExtensions
    {
        public static PatientDetailDTO ADto(this Patient patient)
        {
            var dto = new PatientDetailDTO
            {
                Id = patient.Id,
                Name = patient.Name,
                Email = patient.Email.Value
            };
            return dto;
        }
    }
}
