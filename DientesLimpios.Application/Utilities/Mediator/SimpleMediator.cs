using DientesLimpios.Application.Exceptions;
using DientesLimpios.Application.Utilities.ResultPattern;
using DientesLimpios.Domain.Common.ResultPattern;
using FluentValidation;

namespace DientesLimpios.Application.Utilities.Mediator
{
    public class SimpleMediator : IMediator
    {
        private readonly IServiceProvider _serviceProvider;

        public SimpleMediator(IServiceProvider serviceProvider)
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
                    return CreateValidationFailureResult<TResponse>(validationFailure);

            // Step 2: dispatch.
            var handlerType = typeof(IRequestHandler<,>)
                .MakeGenericType(request.GetType(), typeof(TResponse));

            var handler = _serviceProvider.GetService(handlerType)
                ?? throw new MediatorException(
                    $"No handler found for {request.GetType().Name}");

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
