using FluentValidation;

namespace DientesLimpios.Application.UseCases.Offices.Commands.DeleteOffice
{
    public class DeleteOfficeCommandValidator : AbstractValidator<DeleteOfficeCommand>
    {
        public DeleteOfficeCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotNull().WithMessage("The {PropertyName} field is required")
                .NotEmpty().WithMessage("The {PropertyName} field must have a valid value");
        }
    }
}
