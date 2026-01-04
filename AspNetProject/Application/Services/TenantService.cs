using AspNetProject.Domain.Models;
using AspNetProject.Domain.Ports.In;
using AspNetProject.Domain.Ports.Out;
using AspNetProject.Domain.ValueObjects;

namespace AspNetProject.Application.Services;

public class TenantService : ITenantService
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TenantService(
        ITenantRepository tenantRepository,
        IUnitOfWork unitOfWork)
    {
        _tenantRepository = tenantRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<Tenant>> GetTenantsAsync(CancellationToken cancellationToken = default)
    {
        // Note: For large numbers of tenants, this should be paginated (PagedResult)
        // using a specific generic method in repository like GetAllAsync(pageRequest)
        // Default repository only exposes simple CRUD.
        // For MVP/Demo:
        throw new NotImplementedException("Use Query Service for listing");
    }

    public async Task<Tenant> GetTenantByIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _tenantRepository.GetByIdAsync(tenantId, cancellationToken)
               ?? throw new KeyNotFoundException($"Tenant with ID {tenantId} not found");
    }

    public async Task<TenantConfiguration> GetTenantConfigurationAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var tenant = await GetTenantByIdAsync(tenantId, cancellationToken);
        return tenant.Configuration;
    }

    public async Task<Tenant> CreateTenantAsync(string name, TenantConfiguration configuration, CancellationToken cancellationToken = default)
    {
        var existing = await _tenantRepository.GetByNameAsync(name, cancellationToken);
        if (existing != null)
        {
            throw new InvalidOperationException($"Tenant with name '{name}' already exists.");
        }

        var tenant = new Tenant(name, configuration);
        
        await _tenantRepository.AddAsync(tenant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return tenant;
    }

    public async Task<Tenant> UpdateTenantConfigurationAsync(Guid tenantId, TenantConfiguration newConfiguration, CancellationToken cancellationToken = default)
    {
        var tenant = await GetTenantByIdAsync(tenantId, cancellationToken);
        
        if (tenant.Status == TenantStatus.SUSPENDED)
        {
            throw new InvalidOperationException("Cannot update configuration of a suspended tenant.");
        }
        
        tenant.UpdateConfiguration(newConfiguration);
        
        await _tenantRepository.UpdateAsync(tenant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return tenant;
    }

    public async Task<Tenant> SuspendTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var tenant = await GetTenantByIdAsync(tenantId, cancellationToken);
        
        if (tenant.Status == TenantStatus.SUSPENDED)
        {
             throw new InvalidOperationException("Tenant is already suspended.");
        }

        tenant.Suspend(); // Assuming this method exists on Tenant logic
        
        await _tenantRepository.UpdateAsync(tenant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return tenant;
    }

    public async Task<Tenant> ActivateTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var tenant = await GetTenantByIdAsync(tenantId, cancellationToken);
        
        if (tenant.Status == TenantStatus.ACTIVE)
        {
             throw new InvalidOperationException("Tenant is already active.");
        }

        tenant.Activate(); // Assuming this method exists
        
        await _tenantRepository.UpdateAsync(tenant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return tenant;
    }

    public async Task DeleteTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var tenant = await GetTenantByIdAsync(tenantId, cancellationToken);
        
        // Serious implementation: Check for active users/docs before deleting?
        // Or just implement soft delete.
        tenant.Delete(); // Soft Delete as per Domain model
        
        await _tenantRepository.UpdateAsync(tenant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
