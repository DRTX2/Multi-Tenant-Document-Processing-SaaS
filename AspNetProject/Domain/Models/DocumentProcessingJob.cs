using AspNetProject.Domain.ValueObjects;

namespace AspNetProject.Domain.Models;

public class DocumentProcessingJob : IEntity<Guid>
{
    public Guid Id { get; private set; }
    public Guid DocumentId { get; private set; }
    
    public ProcessingStatus Status { get; private set; }
    public int Attempts { get; private set; }

    public string? LastError { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    private DocumentProcessingJob() { } // For EF Core

    public DocumentProcessingJob(Guid documentId)
    {
        Id = Guid.NewGuid();
        DocumentId = documentId;
        Status = ProcessingStatus.QUEUED;
        Attempts = 0;
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkAsProcessing()
    {
        Status = ProcessingStatus.PROCESSING;
        Attempts++;
    }

    public void MarkAsCompleted()
    {
        Status = ProcessingStatus.COMPLETED;
        CompletedAt = DateTime.UtcNow;
    }

    public void MarkAsFailed(string error)
    {
        Status = ProcessingStatus.FAILED;
        LastError = error;
        CompletedAt = DateTime.UtcNow;
    }
}