using ECommerce.Application.Abstractions;
using ECommerce.Application.Abstractions.Configuration;
using ECommerce.Application.Abstractions.Messaging;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Features.Registration.Events;
using ECommerce.Domain.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Features.Registration.VerifyRegistration;

public record VerifyRegistrationCommand(string Email, string Code) : ICommand<VerifyRegistrationResponse>;

public record VerifyRegistrationResponse(bool Success, string Message);

internal class VerifyRegistrationCommandHandler() : ICommandHandler<VerifyRegistrationCommand, VerifyRegistrationResponse>
{
    public async Task<VerifyRegistrationResponse> Handle(VerifyRegistrationCommand request, CancellationToken cancellationToken) => new VerifyRegistrationResponse(true, "temp");
}
