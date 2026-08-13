using ECommerce.Application.Common.Errors;
using FluentResults;
using global::MediatR;

namespace ECommerce.Application.Common.Behaviours;

/// <summary>
/// Fails fast if a handler returns a raw FluentResults Error instead of a
/// ValidationError or ApplicationError subtype. Must run AFTER ValidationBehaviour
/// so it only inspects results that came out of a handler, not validation failures.
/// </summary>
public sealed class UntypedErrorGuardBehaviour<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var response = await next();

        if (response is ResultBase { IsFailed: true } result)
        {
            var untyped = result.Errors
                .Where(e => e is not ValidationError and not ApplicationError)
                .ToList();

            if (untyped.Count > 0)
            {
                throw new InvalidOperationException(
                    $"{typeof(TRequest).Name} returned untyped Error(s): " +
                    string.Join("; ", untyped.Select(e => e.Message)) +
                    $". Use {nameof(ValidationError)} or an {nameof(ApplicationError)} subtype.");
            }
        }

        return response;
    }
}
