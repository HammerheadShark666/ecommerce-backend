using FluentResults;
using MediatR;

namespace ECommerce.Application.Abstractions.Messaging;

public interface IQuery<TResponse>
    : IRequest<Result<TResponse>>;

public interface IQueryHandler<in TQuery, TResponse>
    : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>;
