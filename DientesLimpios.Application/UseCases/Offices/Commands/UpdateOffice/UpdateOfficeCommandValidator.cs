using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;

namespace DientesLimpios.Application.UseCases.Offices.Commands.UpdateOffice
{
    public class UpdateOfficeCommandValidator : AbstractValidator<UpdateOfficeCommand>
    {
        public UpdateOfficeCommandValidator()
        {
            RuleFor(p => p.Name)
            .NotEmpty().WithMessage("El campo {PropertyName} es requerido")
            .MaximumLength(150).WithMessage("La lontigud del campo {PropertyName} debe ser menor o igual a {MaxLength}");
        }
    }
}
