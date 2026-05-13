namespace DientesLimpios.Application.UseCases.Patients.Queries.GetPatientList
{
    public class PatientFilterDTO
    {
        public int Pagina { get; set; } = 1;
        public int RegistrosPorPagina { get; set; } = 10;
        public string? Name { get; set; }
        public string? Email { get; set; }
    }
}
