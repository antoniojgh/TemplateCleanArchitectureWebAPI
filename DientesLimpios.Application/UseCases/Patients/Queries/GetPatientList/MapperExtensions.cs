using DientesLimpios.Domain.Entities;

namespace DientesLimpios.Application.UseCases.Patients.Queries.GetPatientList
{
    public static class MapperExtensions
    {
        public static PatientListDTO ADto(this Patient patient)
        {
            var dto = new PatientListDTO
            {
                Id = patient.Id,
                Name = patient.Name,
                Email = patient.Email.Value
            };
            return dto;
        }
    }

}
