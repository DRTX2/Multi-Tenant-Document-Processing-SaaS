using AspNetProject.Application.DTOs;
using AspNetProject.Domain.Ports.In;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace AspNetProject.Adapters.In.Controllers;

[ApiController]
[Route("api/tenants")]
public class TenantController : ControllerBase
{
    private readonly ITenantService _tenantService;
    private readonly IValidator<CreateTenantRequest> _createValidator;

    public TenantController(ITenantService tenantService, IValidator<CreateTenantRequest> createValidator)
    {
        _tenantService = tenantService;
        _createValidator = createValidator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateTenant([FromBody] CreateTenantRequest request)
    {
        var validationResult = await _createValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));
        }

        try 
        {
            var config = new AspNetProject.Domain.ValueObjects.TenantConfiguration(
                request.MaxStorageMb, request.OcrEnabled, request.RateLimitPerMinute);
            
            var tenant = await _tenantService.CreateTenantAsync(request.Name, config);
            
            // Map to response DTO ideally
            return CreatedAtAction(nameof(GetTenant), new { id = tenant.Id }, new 
            {
                tenant.Id,
                tenant.Name,
                tenant.Status
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTenant(Guid id)
    {
        try
        {
            var tenant = await _tenantService.GetTenantByIdAsync(id);
            return Ok(new 
            {
                tenant.Id,
                tenant.Name,
                Status = tenant.Status.ToString(),
                tenant.Configuration,
                tenant.CreatedAt
            });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
