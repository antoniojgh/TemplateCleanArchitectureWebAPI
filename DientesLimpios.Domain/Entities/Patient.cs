using DientesLimpios.Domain.Common;
using DientesLimpios.Domain.Common.ResultPattern;
using DientesLimpios.Domain.Errors;
using DientesLimpios.Domain.ValueObjects;

namespace DientesLimpios.Domain.Entities
{
    public class Patient : AggregateRoot 
    {
        public const int NameMaxLength = 250;
        public string Name { get; private set; } = null!;
        public Email Email { get; private set; } = null!;

        private Patient() { }   // EF Core

        private Patient(string name, Email email) : base(Guid.CreateVersion7())
        {
            Name = name;
            Email = email;
        }

        public static Result<Patient> Create(string name, string email)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Result.Failure<Patient>(DomainErrors.Patient.NameRequired);

            if (name.Trim().Length > NameMaxLength)
                return Result.Failure<Patient>(DomainErrors.Patient.NameTooLong);

            var emailResult = Email.Create(email);
            if (emailResult.IsFailure)
                return Result.Failure<Patient>(emailResult.Error);

            return Result.Success(new Patient(name.Trim(), emailResult.Value));
        }

        public Result UpdateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Result.Failure(DomainErrors.Patient.NameRequired);

            if (name.Trim().Length > NameMaxLength)
                return Result.Failure(DomainErrors.Patient.NameTooLong);

            Name = name.Trim();
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
