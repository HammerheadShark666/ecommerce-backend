using System.Collections.Concurrent;
using System.Reflection;
using ECommerce.Application.Common.Errors;
using FluentResults;
using FluentValidation;
using MediatR;

namespace ECommerce.Application.Common.Behaviours;

public sealed class ValidationBehaviour<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private static readonly ConcurrentDictionary<Type, Func<List<IError>, object>> FailFactories = new();

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
        var failures = (await Task.WhenAll(
                validators.Select(v => v.ValidateAsync(context, cancellationToken))))
            .SelectMany(r => r.Errors)
            .ToList();

        if (failures.Count == 0)
        {
            return await next();
        }

        var errors = failures
            .Select(f => (IError)new ValidationError(f.PropertyName, f.ErrorMessage))
            .ToList();

        return (TResponse)CreateFailedResult(typeof(TResponse), errors);
    }

    private static object CreateFailedResult(Type responseType, List<IError> errors)
    {
        var factory = FailFactories.GetOrAdd(responseType, static type =>
        {
            if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(Result<>))
            {
                throw new InvalidOperationException(
                    $"{type.Name} must be Result<T> to use {nameof(ValidationBehaviour<object, object>)}.");
            }

            var failMethod = typeof(Result)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Single(m => m.Name == nameof(Result.Fail)
                             && m.IsGenericMethodDefinition
                             && m.GetParameters() is [var p]
                             && p.ParameterType == typeof(IEnumerable<IError>))
                .MakeGenericMethod(type.GetGenericArguments()[0]);

            return errs => failMethod.Invoke(null, [errs])!;
        });

        return factory(errors);
    }
}
