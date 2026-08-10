using ECommerce.Application.Common.Errors;
using ECommerce.Application.Constants;
using FluentResults;
using Microsoft.AspNetCore.Http;

namespace ECommerce.Application.Extensions;

public static class ResultExtensions
{
    public static IResult ToHttpResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Results.Ok(result.Value);
        }

        var apiError = result.Errors
            .OfType<ApiError>()
            .FirstOrDefault();

        if (apiError is not null)
        {
            return Results.Problem(
                statusCode: (int)apiError.Metadata[ErrorMetadataKeys.StatusCode], 
                detail: apiError.Message);
        }

        return Results.BadRequest(new
        {
            errors = result.Errors.Select(x => x.Message)
        });
    }
}
