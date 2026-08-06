using FluentResults;
using FluentValidation;
using MediatR;
namespace ECommerce.Application.MediatR;

public sealed class ValidationBehaviour<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, TResponse>
                                                                                                        where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> validators = validators;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            validators.Select(v =>
                v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(x => x.Errors)
            .Where(x => x is not null)
            .ToList();

        if (failures.Count > 0)
        {
            var errors = failures
                 .Select(x => (IError)new Error(x.ErrorMessage))
                 .ToList();

            return CreateFailureResult(errors);
        }

        return await next();
    }

    private static TResponse CreateFailureResult(
        List<IError> errors)
    {
        // TResponse is Result<T>
        var resultType = typeof(TResponse);

        if (!resultType.IsGenericType ||
            resultType.GetGenericTypeDefinition() != typeof(Result<>))
        {
            throw new InvalidOperationException(
                "ValidationBehaviour requires Result<T> responses.");
        }

        var responseType = resultType.GetGenericArguments()[0];

        var failMethod = typeof(Result)
            .GetMethods()
            .Single(x =>
                x.Name == nameof(Result.Fail) &&
                x.IsGenericMethod &&
                x.GetParameters().Length == 1 &&
                x.GetParameters()[0].ParameterType == typeof(IEnumerable<IError>));

        var genericFailMethod = failMethod.MakeGenericMethod(responseType);

        return (TResponse)genericFailMethod.Invoke(
            null,
            new object[] { errors })!;
    }
}
