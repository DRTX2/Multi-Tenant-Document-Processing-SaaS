using AspNetProject.Application.DTOs;
using FluentValidation;

namespace AspNetProject.Application.Validators.Tenants;

public class UpdateTenantConfigurationValidator : AbstractValidator<UpdateTenantConfigurationRequest>
{
    public UpdateTenantConfigurationValidator()
    {
        RuleFor(x => x.MaxStorageMb)
            .GreaterThan(0).WithMessage("Max storage must be greater than 0");
        
        RuleFor(x => x.RateLimitPerMinute)
            .GreaterThan(0).WithMessage("Rate limit must be greater than 0");
    }
}
