namespace DientesLimpios.Application.Utilities.Common
{
    public class PagedDTO<T>
    {
        public List<T> Elements { get; set; } = [];
        public int Total { get; set; }
    }
}
