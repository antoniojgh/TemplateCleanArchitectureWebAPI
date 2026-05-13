using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;

namespace DientesLimpios.Application.UseCases.Offices.Commands.CreateOffice
{
    public class CreateOfficeCommandValidator :AbstractValidator<CreateOfficeCommand>
    {
        public CreateOfficeCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El campo {PropertyName} es requerido")
                .MaximumLength(150).WithMessage("La lontigud del campo {PropertyName} debe ser menor o igual a {MaxLength}");
        }
    }
}
