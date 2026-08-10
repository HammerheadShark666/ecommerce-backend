using ECommerce.Application.Constants;
using FluentResults;
using Microsoft.AspNetCore.Http;

namespace ECommerce.Application.Common;

//public static class ResultExtensions
//{
//    public static IResult ToHttpResult<T>(this Result<T> result)
//    {
//        if (result.IsSuccess)
//        {
//            return Results.Ok(result.Value);
//        }

//        var error = result.Errors.First();
//        var statusCode = error.Metadata.TryGetValue(ErrorMetadataKeys.StatusCode, out object? code)
//            ? (int)code
//            : StatusCodes.Status500InternalServerError;

//        return Results.Problem(
//            detail: error.Message,
//            statusCode: statusCode);
//    }
//}
