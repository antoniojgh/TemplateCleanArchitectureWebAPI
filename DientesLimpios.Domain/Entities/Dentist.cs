using DientesLimpios.Domain.Common;
using DientesLimpios.Domain.Common.ResultPattern;
using DientesLimpios.Domain.Errors;
using DientesLimpios.Domain.ValueObjects;

namespace DientesLimpios.Domain.Entities
{
    public class Dentist : AggregateRoot
    {
        public string Name { get; private set; } = null!;
        public Email Email { get; private set; } = null!;

        private Dentist() { } // EF Core

        private Dentist(string name, Email email) : base(Guid.CreateVersion7())
        {
            Name = name;
            Email = email;
        }
        public static Result<Dentist> Create(string name, string email)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Result.Failure<Dentist>(DomainErrors.Dentist.NameRequired);

            var emailResult = Email.Create(email);
            if (emailResult.IsFailure)
                return Result.Failure<Dentist>(emailResult.Error);

            return Result.Success(new Dentist(name, emailResult.Value));
        }
        public Result UpdateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Result.Failure(DomainErrors.Dentist.NameRequired);

            Name = name;
            return Result.Success();
        }
        public Result UpdateEmail(string email)
        {
            var emailResult = Email.Create(email);
            if (emailResult.IsFailure)
                return Result.Failure(emailResult.Error);

            Email = emailResult.Value;
            return Result.Success();
        }

    }
}
