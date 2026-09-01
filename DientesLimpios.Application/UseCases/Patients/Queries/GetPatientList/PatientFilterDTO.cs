namespace DientesLimpios.Application.UseCases.Patients.Queries.GetPatientList
{
    public class PatientFilterDTO
    {
        public int Page { get; set; } = 1;
        public int RecordsPerPage { get; set; } = 10;
        public string? Name { get; set; }
        public string? Email { get; set; }
    }
}
