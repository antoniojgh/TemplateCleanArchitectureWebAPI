using DientesLimpios.Domain.Common.ResultPattern;
using DientesLimpios.Domain.Errors;

namespace DientesLimpios.Domain.ValueObjects
{
    public sealed record Email
    {
        public string Value { get; private init; } = null!;

        private Email() { }   // EF Core

        private Email(string valor) => Value = valor;

        public static Result<Email> Create(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return Result.Failure<Email>(DomainErrors.Email.Empty);

            if (!valor.Contains('@', StringComparison.Ordinal))
                return Result.Failure<Email>(DomainErrors.Email.InvalidFormat);

            return Result.Success(new Email(valor));
        }
    }

}
