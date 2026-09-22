using System.Net.Mail;
using DientesLimpios.Domain.Common.ResultPattern;
using DientesLimpios.Domain.Errors;

namespace DientesLimpios.Domain.ValueObjects
{
    public sealed record Email
    {
        // RFC 5321: the longest address a server must accept. Persistence and the API
        // length rules read this instead of repeating the number.
        public const int MaxLength = 254;

        public string Value { get; private init; } = null!;

        private Email() { }   // EF Core

        private Email(string value)
        {
            Value = value;
        }

        public static Result<Email> Create(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return Result.Failure<Email>(DomainErrors.Email.Empty);

            var normalized = value.Trim();

            if (normalized.Length > MaxLength)
                return Result.Failure<Email>(DomainErrors.Email.TooLong);

            // MailAddress also parses "Display Name <a@b.com>", which is not an address;
            // requiring the parse to return the input unchanged rejects that form.
            if (!MailAddress.TryCreate(normalized, out var parsed) ||
                !string.Equals(parsed.Address, normalized, StringComparison.OrdinalIgnoreCase))
                return Result.Failure<Email>(DomainErrors.Email.InvalidFormat);

            // Stored lower-cased so two spellings of one mailbox are one value.
            return Result.Success(new Email(normalized.ToLowerInvariant()));
        }
    }

}
