using AspNetProject.Domain.Models;
using AspNetProject.Domain.Ports.In;
using AspNetProject.Domain.Ports.Out;

namespace AspNetProject.Application.Services;

public class AuditService : IAuditService
{
    private readonly IAuditRepository _auditRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AuditService(IAuditRepository auditRepository, IUnitOfWork unitOfWork)
    {
        _auditRepository = auditRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuditLog> LogUserActionAsync(Guid tenantId, string action, string resource, string ipAddress, Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        var log = new AuditLog(tenantId, action, resource, ipAddress, userId);
        
        await _auditRepository.AddAsync(log, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return log;
    }

    public async Task<IEnumerable<AuditLog>> GetAuditRecordsAsync(Guid? tenantId = null, Guid? userId = null, DateTime? startDate = null,
        DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        return await _auditRepository.GetWithFiltersAsync(tenantId, userId, startDate, endDate, null, null, cancellationToken);
    }

    public async Task<IEnumerable<AuditLog>> GetAuditRecordsByResourceAsync(string resource, Guid tenantId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(resource)) throw new ArgumentNullException(nameof(resource));
        
        return await _auditRepository.GetWithFiltersAsync(tenantId, null, null, null, resource, null, cancellationToken);
    }

    public async Task<IEnumerable<AuditLog>> GetAuditRecordsByActionAsync(string action, Guid tenantId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(action)) throw new ArgumentNullException(nameof(action));
        
        return await _auditRepository.GetWithFiltersAsync(tenantId, null, null, null, null, action, cancellationToken);
    }
}
