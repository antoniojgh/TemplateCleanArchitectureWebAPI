using DientesLimpios.Aplicacion.Excepciones;
using FluentValidation;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace DientesLimpios.Aplicacion.Behaviors
{
    // This class intercepts ANY request (TRequest) that returns a response (TResponse)
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        // We inject IEnumerable to handle cases where you might have 0, 1, or multiple validators for a command
        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            // 1. If there are no validators for this command, just continue.
            if (!_validators.Any())
            {
                return await next();
            }

            // 2. Create the validation context
            var context = new ValidationContext<TRequest>(request);

            // 3. Run all validators asynchronously
            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken))
            );

            // 4. Collect all failures from all validators
            var failures = validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            // 5. If there are failures, stop the pipeline and throw your custom exception
            if (failures.Count != 0)
            {
                // We recreate the ValidationResult object your exception expects
                var finalResult = new FluentValidation.Results.ValidationResult(failures);
                throw new ExcepcionDeValidacion(finalResult);
            }

            // 6. If valid, continue to the next step (your Handler)
            return await next();
        }
    }
}
