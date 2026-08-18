using ECommerce.Application.Abstractions.Messaging;

namespace ECommerce.Application.Features.Admin.User.Role;

public record AddUserRoleRequest(Guid UserId, Guid RoleId);

public record AddUserRoleCommand(Guid UserId, Guid RoleId) : ICommand<AddUserRoleResponse>;

public record AddUserRoleResponse();
