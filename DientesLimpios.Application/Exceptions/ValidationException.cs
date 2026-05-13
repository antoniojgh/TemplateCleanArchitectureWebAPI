using FluentValidation.Results;

namespace DientesLimpios.Application.Exceptions
{
    public class ValidationException: Exception
    {
        public List<string> ErrorsDeValidacion { get; set; } = new List<string>();

        public ValidationException(string mensajeDeError) : base(mensajeDeError)
        {
            ErrorsDeValidacion.Add(mensajeDeError);
        }


        // Join the errors into a single string and pass it to base()
        public ValidationException(ValidationResult validationResult)
            : base(string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)))
        {
            foreach (var errorDeValidacion in validationResult.Errors)
            {
                ErrorsDeValidacion.Add(errorDeValidacion.ErrorMessage);
            }
        }
    }
}
