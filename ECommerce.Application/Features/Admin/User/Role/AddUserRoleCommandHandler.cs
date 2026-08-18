using ECommerce.Application.Abstractions;
using ECommerce.Application.Abstractions.Messaging;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Features.Admin.User.Role;

internal class AddUserRoleCommandHandler(IECommerceDbContext dbContext) : ICommandHandler<AddUserRoleCommand, AddUserRoleResponse>
{
    public async Task<Result<AddUserRoleResponse>> Handle(AddUserRoleCommand request, CancellationToken cancellationToken) 
                                => Result.Ok(new AddUserRoleResponse("If an account exists for that email, a reset link has been sent."));

    private Task<Domain.Entities.User.User?> GetUserAsync(string email, CancellationToken cancellationToken) => dbContext.Users
                            .AsNoTracking()
                            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
}
