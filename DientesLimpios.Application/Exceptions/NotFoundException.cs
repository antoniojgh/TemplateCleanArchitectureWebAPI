namespace DientesLimpios.Application.Exceptions
{
    public class NotFoundException : Exception
    {
        public string EntityType { get; }
        public object Identifier { get; }

        public NotFoundException(string entityType, object id)
            : base($"{entityType} with id '{id}' not found.")
        {
            EntityType = entityType;
            Identifier = id;
        }
    }
}