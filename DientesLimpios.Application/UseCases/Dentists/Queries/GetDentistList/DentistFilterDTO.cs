namespace DientesLimpios.Application.UseCases.Dentists.Queries.GetDentistList
{
    public class DentistFilterDTO
    {
        public int Page { get; set; } = 1;
        public int RecordsPerPage { get; set; } = 10;
        public string? Name { get; set; }
        public string? Email { get; set; }
    }
}
