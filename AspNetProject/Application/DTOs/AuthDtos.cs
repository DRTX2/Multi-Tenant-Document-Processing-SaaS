using System.ComponentModel.DataAnnotations;

namespace AspNetProject.Application.DTOs;

public record LoginRequest(
    [Required] [EmailAddress] string Email, 
    [Required] string Password,
    [Required] Guid TenantId
);

public record LoginResponse(
    string Token
);
