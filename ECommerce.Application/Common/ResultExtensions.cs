using FluentResults;
using Microsoft.AspNetCore.Http;

namespace ECommerce.Application.Common;

public static class ResultExtensions
{
    public static IResult ToHttpResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Results.Ok(result.Value);
        }

        IError error = result.Errors.First();
        int statusCode = error.Metadata.TryGetValue("StatusCode", out object? code)
            ? (int)code
            : StatusCodes.Status500InternalServerError;

        return Results.Problem(
            detail: error.Message,
            statusCode: statusCode);
    }
}
