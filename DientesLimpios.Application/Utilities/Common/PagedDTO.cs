namespace DientesLimpios.Application.Utilities.Common
{
    public class PagedDTO<T>
    {
        public List<T> Elementos { get; set; } = [];
        public int Total { get; set; }
    }
}
