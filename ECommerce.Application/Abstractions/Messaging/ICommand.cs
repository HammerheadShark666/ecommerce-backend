using MediatR;

namespace ECommerce.Application.Abstractions.Messaging;

public interface ICommand<out TResponse> : IRequest<TResponse>; 

public interface ICommandHandler<in TCommand, TResponse>
    : IRequestHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>;
