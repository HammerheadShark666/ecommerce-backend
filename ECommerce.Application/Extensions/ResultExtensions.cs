using System.Text.Json;
using ECommerce.Application.Common.Errors;
using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;

namespace ECommerce.Application.Extensions;

public static class ResultExtensions
{
    public static IResult ToHttpResult<T>(this Result<T> result) =>
        result.IsSuccess ? Results.Ok(result.Value) : ToProblemResult(result);

    public static IResult ToHttpResult(this Result result) =>
        result.IsSuccess ? Results.Ok() : ToProblemResult(result);

    private static IResult ToProblemResult(ResultBase result)
    {
        var validationErrors = result.Errors.OfType<ValidationError>().ToList();
        if (validationErrors.Count > 0)
        {
            var errors = validationErrors
                .GroupBy(e => JsonNamingPolicy.CamelCase.ConvertName(e.PropertyName))
                .ToDictionary(g => g.Key, g => g.Select(e => e.Message).ToArray());

            return Results.ValidationProblem(errors);
        }

        var appError = result.Errors.OfType<ApplicationError>().FirstOrDefault();
        if (appError is not null)
        {
            return Results.Problem(
                statusCode: appError.StatusCode,
                title: ReasonPhrases.GetReasonPhrase(appError.StatusCode),
                detail: appError.Message);
        }

        // Untyped Error reached the HTTP layer. UntypedErrorGuardBehaviour should have
        // thrown before this point for anything running through MediatR — if you land
        // here, either that behaviour isn't registered, or this result didn't go through
        // the pipeline. Treat as a bug, not a client fault.
        var errorId = Guid.NewGuid();

        return Results.Problem(
            statusCode: StatusCodes.Status500InternalServerError,
            title: "An unexpected error occurred.",
            extensions: new Dictionary<string, object?> { ["errorId"] = errorId });
    }
}
