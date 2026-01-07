using System;
using System.Collections.Generic;
using System.Text;
using DientesLimpios.Aplicacion.CasosdeUso.Pacientes.Comandos.CrearPaciente;
using FluentValidation;

namespace DientesLimpios.Aplicacion.CasosdeUso.Dentistas.Comandos.CrearDentista
{
    public class ValidadorComandoCrearDentista : AbstractValidator<ComandoCrearDentista>
    {
        public ValidadorComandoCrearDentista()
        {
            RuleFor(p => p.Nombre)
        .NotEmpty().WithMessage("El campo {PropertyName} es requerido")
        .MaximumLength(250).WithMessage("La lontigud del campo {PropertyName} debe ser menor o igual a {MaxLength}");

            RuleFor(p => p.Email)
        .NotEmpty().WithMessage("El campo {PropertyName} es requerido")
        .MaximumLength(254).WithMessage("La lontigud del campo {PropertyName} debe ser menor o igual a {MaxLength}")
        .EmailAddress().WithMessage("El formato del email no es válido");

        }
    }
}
