using AspNetProject.Domain.Models;
using AspNetProject.Domain.Ports.In;
using AspNetProject.Domain.Ports.Out;
using AspNetProject.Domain.ValueObjects;

namespace AspNetProject.Application.Services;

public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IFileStorage _fileStorage;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDomainEventDispatcher _eventDispatcher;

    public DocumentService(
        IDocumentRepository documentRepository,
        IFileStorage fileStorage,
        IUnitOfWork unitOfWork,
        IDomainEventDispatcher eventDispatcher)
    {
        _documentRepository = documentRepository;
        _fileStorage = fileStorage;
        _unitOfWork = unitOfWork;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<IEnumerable<Document>> GetDocumentsByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _documentRepository.GetByTenantIdAsync(tenantId, cancellationToken);
    }

    public async Task<Document> GetDocumentByIdAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        return await _documentRepository.GetByIdAsync(documentId, cancellationToken)
               ?? throw new KeyNotFoundException($"Document with ID {documentId} not found.");
    }

    public async Task<Document> UploadDocumentAsync(Guid tenantId, Guid ownerUserId, DocumentMetadata metadata, byte[] content,
        CancellationToken cancellationToken = default)
    {
        // 1. Create Domain Entity
        var document = new Document(tenantId, ownerUserId, metadata);

        // 2. Upload File to Storage (Strategy: Upload before DB to ensure file exists)
        // Store using a secure path structure: tenantId/documentId
        var storagePath = $"{tenantId}/{document.Id}";
        using var stream = new MemoryStream(content);
        await _fileStorage.UploadAsync(storagePath, stream, cancellationToken);

        // 3. Persist Entity
        await _documentRepository.AddAsync(document, cancellationToken);
        
        // 4. Save Changes (This commits the transaction)
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        // 5. Dispatch Domain Events (After successful commit)
        // Note: In a more advanced setup, use Outbox Pattern here.
        foreach (var domainEvent in document.DomainEvents)
        {
            await _eventDispatcher.DispatchAsync(domainEvent, cancellationToken);
        }

        return document;
    }

    public async Task<IEnumerable<DocumentVersion>> GetDocumentVersionsAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        var document = await GetDocumentByIdAsync(documentId, cancellationToken);
        return document.GetVersions();
    }

    public async Task<Document> UpdateDocumentMetadataAsync(Guid documentId, DocumentMetadata newMetadata, CancellationToken cancellationToken = default)
    {
        var document = await GetDocumentByIdAsync(documentId, cancellationToken);
        
        document.UpdateMetadata(newMetadata);
        
        await _documentRepository.UpdateAsync(document, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return document;
    }

    public async Task SoftDeleteDocumentAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        var document = await GetDocumentByIdAsync(documentId, cancellationToken);
        
        document.SoftDelete();
        
        await _documentRepository.UpdateAsync(document, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<Document> RestoreDocumentAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        var document = await GetDocumentByIdAsync(documentId, cancellationToken);
        // Logic to restore
        
        await _documentRepository.UpdateAsync(document, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return document;
    }
}
