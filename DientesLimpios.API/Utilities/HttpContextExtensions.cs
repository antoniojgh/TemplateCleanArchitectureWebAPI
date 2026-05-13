namespace DientesLimpios.API.Utilities
{
    public static class HttpContextExtensions
    {
        public static void InsertPaginationInHeader(this HttpContext httpContext, int totalRecordCount)
        {
            httpContext.Response.Headers.Append("cantidad-total-registros", totalRecordCount.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }
    }
}
