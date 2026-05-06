using DientesLimpios.Aplicacion.Excepciones;
using DientesLimpios.Aplicacion.Utilidades.PatronResultados;
using DientesLimpios.Dominio.Comunes.PatronResultados;
using FluentValidation;

namespace DientesLimpios.Aplicacion.Utilidades.Mediador
{
    public class MediadorSimple : IMediator
    {
        private readonly IServiceProvider _serviceProvider;

        public MediadorSimple(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<TResponse> Send<TResponse>(
            IRequest<TResponse> request,
            CancellationToken cancellationToken = default)
        {
            // Step 1: validate. If validation fails AND the response type is a Result,
            // short-circuit by returning Result.Failure(ValidationError).
            var validationFailure = await TryValidate(request, cancellationToken);
            if (validationFailure is not null)
            {
                // If TResponse is a Result type, build a failure result.
                if (typeof(Result).IsAssignableFrom(typeof(TResponse)))
                    return CreateValidationFailureResult<TResponse>(validationFailure);

                // If TResponse is not a Result type (legacy code path),
                // fall back to throwing. After full migration, this branch
                // should never execute.
                throw new ExcepcionDeValidacion(string.Join("; ",
                    validationFailure.Errors.Select(e => e.Message)));
            }

            // Step 2: dispatch.
            var handlerType = typeof(IRequestHandler<,>)
                .MakeGenericType(request.GetType(), typeof(TResponse));

            var handler = _serviceProvider.GetService(handlerType)
                ?? throw new ExcepcionDeMediador(
                    $"No se encontró un handler para {request.GetType().Name}");

            var handleMethod = handlerType.GetMethod("Handle")!;
            return await (Task<TResponse>)handleMethod.Invoke(
                handler, new object[] { request, cancellationToken })!;
        }

        private async Task<ValidationError?> TryValidate(
            object request, CancellationToken cancellationToken)
        {
            var validatorType = typeof(IValidator<>).MakeGenericType(request.GetType());
            var validator = (IValidator?)_serviceProvider.GetService(validatorType);

            if (validator is null)
                return null;

            var context = new ValidationContext<object>(request);
            var validationResult = await validator.ValidateAsync(context, cancellationToken);

            if (validationResult.IsValid)
                return null;

            var errors = validationResult.Errors
                .Select(f => new Error(f.PropertyName, f.ErrorMessage))
                .ToArray();

            return new ValidationError(errors);
        }

        private static TResponse CreateValidationFailureResult<TResponse>(
            ValidationError validationError)
        {
            // If TResponse is exactly Result, return Result.Failure(validationError).
            if (typeof(TResponse) == typeof(Result))
                return (TResponse)(object)Result.Failure(validationError);

            // If TResponse is Result<T>, use reflection to build Result.Failure<T>(validationError).
            var responseType = typeof(TResponse);
            var valueType = responseType.GetGenericArguments()[0];

            var failureMethod = typeof(Result)
                .GetMethods()
                .First(m => m.Name == nameof(Result.Failure) && m.IsGenericMethod);

            var genericFailure = failureMethod.MakeGenericMethod(valueType);
            var failureResult = genericFailure.Invoke(null, new object[] { validationError })!;

            return (TResponse)failureResult;
        }
    }
}
