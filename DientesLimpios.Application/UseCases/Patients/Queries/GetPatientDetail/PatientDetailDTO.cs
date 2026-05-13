namespace DientesLimpios.Application.UseCases.Patients.Queries.GetPatientDetail
{
    public class PatientDetailDTO
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
    }
}
