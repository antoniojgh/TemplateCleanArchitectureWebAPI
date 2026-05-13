namespace DientesLimpios.Application.UseCases.Dentists.Queries.GetDentistDetail
{
    public class DentistDetailDTO
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
    }
}
