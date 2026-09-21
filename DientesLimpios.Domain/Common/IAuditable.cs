namespace DientesLimpios.Domain.Common
{
    // Audit data is written by the persistence layer (AuditableEntitiesInterceptor) from the
    // ambient user and clock — never by domain code or a handler. It is readable here so a
    // query or DTO can project it; there are deliberately no setters, so a caller that holds
    // an aggregate, or casts it to this interface, still cannot forge the trail.
    public interface IAuditable
    {
        string? CreatedBy { get; }
        DateTime CreatedDate { get; }
        string? LastModifiedBy { get; }
        DateTime? LastModifiedDate { get; }
    }
}
