using System;
using System.Collections.Generic;
using System.Text;
using DientesLimpios.Domain.Common;
using DientesLimpios.Domain.Common.ResultPattern;
using DientesLimpios.Domain.Errors;
using DientesLimpios.Domain.Exceptions;

namespace DientesLimpios.Domain.Entities
{
    public class Office : AuditableEntity
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; } = null!;

        private Office() { }   // EF Core

        private Office(string name)
        {
            Id = Guid.CreateVersion7();
            Name = name;
        }

        public static Result<Office> Create(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Result.Failure<Office>(DomainErrors.Office.NameRequired);

            return Result.Success(new Office(name));
        }

        public Result UpdateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Result.Failure(DomainErrors.Office.NameRequired);

            Name = name;
            return Result.Success();
        }

    }
}
