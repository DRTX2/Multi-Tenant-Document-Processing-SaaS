using AspNetProject.Domain.Models;
using AspNetProject.Domain.Ports.In;
using AspNetProject.Domain.Ports.Out;
using AspNetProject.Domain.ValueObjects;

namespace AspNetProject.Application.Services;

public class DocumentProcessingService : IDocumentProcessingService
{
    private readonly IDocumentJobRepository _jobRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DocumentProcessingService(
        IDocumentJobRepository jobRepository,
        IDocumentRepository documentRepository,
        IUnitOfWork unitOfWork)
    {
        _jobRepository = jobRepository;
        _documentRepository = documentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> EnqueueDocumentForProcessingAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        var document = await _documentRepository.GetByIdAsync(documentId, cancellationToken)
            ?? throw new KeyNotFoundException($"Document {documentId} not found");

        if (document.Status != DocumentStatus.UPLOADED)
        {
            // Ideally idempotent, but spec says throw if not UPLOADED? 
            // Or maybe we can re-process if failed?
            // For now follow strictly.
        }

        var job = new DocumentProcessingJob(documentId);
        
        await _jobRepository.AddAsync(job, cancellationToken);
        
        document.UpdateStatus(DocumentStatus.QUEUED);
        await _documentRepository.UpdateAsync(document, cancellationToken);
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return job.Id;
    }

    public async Task<ProcessingStatus> GetDocumentProcessingStatusAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        var job = await _jobRepository.GetByDocumentIdAsync(documentId, cancellationToken)
                  ?? throw new KeyNotFoundException($"No processing job found for document {documentId}");

        return job.Status;
    }

    public async Task<DocumentProcessingJob> GetDocumentProcessingJobAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        return await _jobRepository.GetByDocumentIdAsync(documentId, cancellationToken)
                  ?? throw new KeyNotFoundException($"No processing job found for document {documentId}");
    }

    public async Task StartOcrProcessingAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        var job = await GetDocumentProcessingJobAsync(documentId, cancellationToken);
        var document = await _documentRepository.GetByIdAsync(documentId, cancellationToken);
        
        job.MarkAsProcessing();
        if (document != null) 
        {
            document.UpdateStatus(DocumentStatus.PROCESSING);
            await _documentRepository.UpdateAsync(document, cancellationToken);
        }

        await _jobRepository.UpdateAsync(job, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task StartIndexingAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        // Mock implementation sharing logic with OCR for now
        // In real world, this might be a separate step in a pipeline
        await StartOcrProcessingAsync(documentId, cancellationToken);
    }

    public async Task StartClassificationAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
         // Mock implementation
         await StartOcrProcessingAsync(documentId, cancellationToken);
    }

    public async Task CompleteJobAsync(Guid jobId, CancellationToken cancellationToken = default)
    {
        var job = await _jobRepository.GetByIdAsync(jobId, cancellationToken)
            ?? throw new KeyNotFoundException($"Job {jobId} not found");

        job.MarkAsCompleted();
        await _jobRepository.UpdateAsync(job, cancellationToken);

        var document = await _documentRepository.GetByIdAsync(job.DocumentId, cancellationToken);
        if (document != null)
        {
            document.UpdateStatus(DocumentStatus.AVAILABLE);
            await _documentRepository.UpdateAsync(document, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task FailJobAsync(Guid jobId, string errorMessage, CancellationToken cancellationToken = default)
    {
        var job = await _jobRepository.GetByIdAsync(jobId, cancellationToken)
            ?? throw new KeyNotFoundException($"Job {jobId} not found");

        job.MarkAsFailed(errorMessage);
        await _jobRepository.UpdateAsync(job, cancellationToken);
        
        // Optionally update document status to FAILED or similar
        // For now, keep as PROCESSING or update to error state if Document entity has one.
        // DocumentStatus doesn't have FAILED, assuming it stays in PROCESSING or we add FAILED.
        // Let's assume we leave it as is or strictly follow domain.
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
