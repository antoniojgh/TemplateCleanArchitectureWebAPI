using FluentValidation.Results;

namespace DientesLimpios.Aplicacion.Excepciones
{
    public class ExcepcionDeValidacion: Exception
    {
        public List<string> ErroresDeValidacion { get; set; } = new List<string>();

        public ExcepcionDeValidacion(string mensajeDeError) : base(mensajeDeError)
        {
            ErroresDeValidacion.Add(mensajeDeError);
        }


        // Join the errors into a single string and pass it to base()
        public ExcepcionDeValidacion(ValidationResult validationResult)
            : base(string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)))
        {
            foreach (var errorDeValidacion in validationResult.Errors)
            {
                ErroresDeValidacion.Add(errorDeValidacion.ErrorMessage);
            }
        }
    }
}
