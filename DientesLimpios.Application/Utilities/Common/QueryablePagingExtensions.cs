using Microsoft.EntityFrameworkCore;

namespace DientesLimpios.Application.Utilities.Common
{
    public static class QueryablePagingExtensions
    {
        public static IQueryable<T> Paginate<T>(this IQueryable<T> query, int page, int recordsPerPage)
        {
            return query
                .Skip((page - 1) * recordsPerPage)
                .Take(recordsPerPage);
        }

        // The caller orders the query first: paging an unordered query returns pages in an
        // undefined order. Total is counted on the filtered query, before Skip/Take.
        public static async Task<PagedDTO<T>> ToPagedAsync<T>(this IQueryable<T> query, int page,
                                                              int recordsPerPage, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(query);

            var total = await query.CountAsync(cancellationToken);
            var items = await query.Paginate(page, recordsPerPage).ToListAsync(cancellationToken);

            return new PagedDTO<T> { Items = items, Total = total };
        }
    }
}
