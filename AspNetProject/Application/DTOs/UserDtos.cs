using AspNetProject.Domain.ValueObjects;

namespace AspNetProject.Application.DTOs;

public record RegisterUserRequest(
    string Email,
    string Password,
    Guid TenantId,
    List<string>? Roles = null
);

public record UpdateUserRequest(
    string? Email,
    string? Password
);

public record UserDto(
    Guid Id,
    Guid TenantId,
    string Email,
    string Status,
    List<string> Roles,
    DateTime CreatedAt
);
