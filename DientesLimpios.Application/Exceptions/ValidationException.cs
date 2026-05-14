using FluentValidation.Results;

namespace DientesLimpios.Application.Exceptions
{
    public class ValidationException: Exception
    {
        public List<string> ValidationErrors { get; set; } = new List<string>();

        public ValidationException(string errorMessage) : base(errorMessage)
        {
            ValidationErrors.Add(errorMessage);
        }


        // Join the errors into a single string and pass it to base()
        public ValidationException(ValidationResult validationResult)
            : base(string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)))
        {
            foreach (var validationError in validationResult.Errors)
            {
                ValidationErrors.Add(validationError.ErrorMessage);
            }
        }
    }
}
