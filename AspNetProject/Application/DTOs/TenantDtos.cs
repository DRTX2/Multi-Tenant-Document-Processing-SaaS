using AspNetProject.Domain.ValueObjects;

namespace AspNetProject.Application.DTOs;

public record CreateTenantRequest(
    string Name,
    int MaxStorageMb = 1024,
    bool OcrEnabled = false,
    int RateLimitPerMinute = 60
);

public record UpdateTenantConfigurationRequest(
    int MaxStorageMb,
    bool OcrEnabled,
    int RateLimitPerMinute
);

public record TenantResponse(
    Guid Id,
    string Name,
    string Status,
    int MaxStorageMb,
    bool OcrEnabled,
    int RateLimitPerMinute,
    DateTime CreatedAt
);
