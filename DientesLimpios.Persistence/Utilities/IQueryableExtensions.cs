namespace DientesLimpios.Persistence.Utilities
{
    public static class IQueryableExtensions
    {
        public static IQueryable<T> Paginar<T>(this IQueryable<T> queryable, int page, int recordsPerPage)
        {
            return queryable
                .Skip((page - 1) * recordsPerPage)
                .Take(recordsPerPage);
        }
    }
}
