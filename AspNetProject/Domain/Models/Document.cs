using AspNetProject.Domain.Events;
using AspNetProject.Domain.ValueObjects;

namespace AspNetProject.Domain.Models;

public class Document : AggregateRoot<Guid>
{
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
        
        AddDomainEvent(new DocumentUploaded(Id, TenantId, Metadata.FileName));
    }

    public void AddVersion(DocumentVersion version)
    {
        if (version == null) throw new ArgumentNullException(nameof(version));
        _versions.Add(version);
    }

    public void UpdateMetadata(DocumentMetadata newMetadata)
    {
        Metadata = newMetadata ?? throw new ArgumentNullException(nameof(newMetadata));
    }

    public void SoftDelete()
    {
        // Add domain rule: Can only delete if not already processing hard tasks? 
        // For now, simple state change.
        Status = DocumentStatus.DELETED;
    }

    public void Restore()
    {
        if (Status != DocumentStatus.DELETED)
        {
            throw new InvalidOperationException("Document is not deleted.");
        }
        Status = DocumentStatus.UPLOADED; // Reset to uploaded or previous state
    }

    public void UpdateStatus(DocumentStatus newStatus)
    {
        Status = newStatus;
    }
}