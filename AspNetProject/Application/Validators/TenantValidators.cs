using AspNetProject.Application.DTOs;
using FluentValidation;

namespace AspNetProject.Application.Validators;

public class CreateTenantValidator : AbstractValidator<CreateTenantRequest>
{
    public CreateTenantValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tenant name is required")
            .MaximumLength(100).WithMessage("Tenant name must not exceed 100 characters");

        RuleFor(x => x.MaxStorageMb)
            .GreaterThan(0).WithMessage("Max storage must be greater than 0");
        
        RuleFor(x => x.RateLimitPerMinute)
            .GreaterThan(0).WithMessage("Rate limit must be greater than 0");
    }
}

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
