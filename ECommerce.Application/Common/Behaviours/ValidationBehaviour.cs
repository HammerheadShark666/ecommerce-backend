using System.Reflection;
using FluentResults;
using FluentValidation;
using global::ECommerce.Application.Common.Errors;
using global::MediatR;

namespace ECommerce.Application.Common.Behaviours;

public sealed class ValidationBehaviour<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
        {
            return await next();
        }

        var failures = validators
            .Select(v => v.Validate(request))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count == 0)
        {
            return await next();
        }

        var errors = failures
            .Select(f => new ValidationError(f.ErrorMessage))
            .Cast<IError>()
            .ToList();

        // TResponse is Result<T> — construct failure via reflection-free generic factory
        var resultType = typeof(TResponse).GetGenericArguments()[0];
        var failMethod = typeof(Result)
            .GetMethods()
            .First(m => m.Name == nameof(Result.Fail) && m.IsGenericMethod && m.GetParameters()[0].ParameterType == typeof(IEnumerable<IError>))
            .MakeGenericMethod(resultType);

        return (TResponse)failMethod.Invoke(null, [errors])!;
    }
}
