namespace DientesLimpios.Application.UseCases.Dentists.Queries.GetDentistList
{
    public class DentistFilterDTO
    {
        public int Pagina { get; set; } = 1;
        public int RegistrosPorPagina { get; set; } = 10;
        public string? Name { get; set; }
        public string? Email { get; set; }
    }
}
