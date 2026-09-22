using DientesLimpios.Domain.Common;
using DientesLimpios.Domain.Common.ResultPattern;
using DientesLimpios.Domain.Errors;

namespace DientesLimpios.Domain.Entities
{
    public class Office : AggregateRoot
    {
        public const int NameMaxLength = 150;
        public string Name { get; private set; } = null!;

        private Office() { }   // EF Core

        private Office(string name) : base(Guid.CreateVersion7())
        {
            Name = name;
        }

        public static Result<Office> Create(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Result.Failure<Office>(DomainErrors.Office.NameRequired);

            if (name.Trim().Length > NameMaxLength)
                return Result.Failure<Office>(DomainErrors.Office.NameTooLong);

            return Result.Success(new Office(name.Trim()));
        }

        public Result UpdateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Result.Failure(DomainErrors.Office.NameRequired);

            if (name.Trim().Length > NameMaxLength)
                return Result.Failure(DomainErrors.Office.NameTooLong);

            Name = name.Trim();
            return Result.Success();
        }

    }
}
