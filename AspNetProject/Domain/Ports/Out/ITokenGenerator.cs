using AspNetProject.Domain.Models;

namespace AspNetProject.Domain.Ports.Out;

public interface ITokenGenerator
{
    string GenerateToken(TenantUser user);
}
