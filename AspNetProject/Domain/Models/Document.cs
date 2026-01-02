using AspNetProject.Domain.ValueObjects;

namespace AspNetProject.Domain.Models;

public class Document : IEntity<Guid>
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid OwnerUserId { get; private set; }
    
    public DocumentMetadata Metadata { get; private set; }
    public DocumentStatus Status { get; private set; }
    
    private readonly List<DocumentVersion> _versions;
    public IReadOnlyCollection<DocumentVersion> GetVersions() => _versions.AsReadOnly();
    
    public DateTime CreatedAt { get; private set; }

    private Document() 
    {
        Metadata = null!;
        _versions = new List<DocumentVersion>();
    } // For EF Core

    public Document(
        Guid tenantId,
        Guid ownerUserId,
        DocumentMetadata metadata)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId;
        OwnerUserId = ownerUserId;
        Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        Status = DocumentStatus.UPLOADED;
        _versions = new List<DocumentVersion>();
        CreatedAt = DateTime.UtcNow;
    }

    public void AddVersion(DocumentVersion version)
    {
        if (version == null) throw new ArgumentNullException(nameof(version));
        _versions.Add(version);
    }

    public void UpdateStatus(DocumentStatus newStatus)
    {
        Status = newStatus;
    }
}