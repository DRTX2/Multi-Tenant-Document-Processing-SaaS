using AspNetProject.Domain.Ports.In;
using AspNetProject.Domain.Ports.Out;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AspNetProject.Infrastructure.BackgroundJobs;

/// <summary>
/// Worker service that polls for queued document processing jobs and executes them.
/// In a real architecture, this might consume messages from RabbitMQ/Azure Service Bus.
/// </summary>
public class DocumentProcessingWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DocumentProcessingWorker> _logger;

    public DocumentProcessingWorker(
        IServiceProvider serviceProvider,
        ILogger<DocumentProcessingWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DocumentProcessingWorker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessNextJobAsync(stoppingToken);
                // Wait before next poll if no job was found or after processing
                if (!stoppingToken.IsCancellationRequested)
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Graceful shutdown
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in DocumentProcessingWorker loop.");
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken); // Backoff on error
            }
        }
        
        _logger.LogInformation("DocumentProcessingWorker stopping.");
    }

    private async Task ProcessNextJobAsync(CancellationToken stoppingToken)
    {
        // Scope is required because repositories are Scoped
        using var scope = _serviceProvider.CreateScope();
        var jobRepository = scope.ServiceProvider.GetRequiredService<IDocumentJobRepository>();
        var processingService = scope.ServiceProvider.GetRequiredService<IDocumentProcessingService>();
        
        // 1. Fetch next queued job
        var job = await jobRepository.GetNextQueuedJobAsync(stoppingToken);
        if (job == null)
        {
            return; // Nothing to process
        }

        _logger.LogInformation("Processing Job {JobId} for Document {DocumentId}", job.Id, job.DocumentId);

        try
        {
            // 2. Start Processing (Update status to PROCESSING)
            await processingService.StartOcrProcessingAsync(job.DocumentId, stoppingToken);

            // 3. Simulate Actual OCR/Analysis Work (CPU bound or External API call)
            // In a real scenario, we might download the file from Storage here.
            await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken); // Simulating work

            // 4. Complete Job (Update status to COMPLETED)
            await processingService.CompleteJobAsync(job.Id, stoppingToken);
            
            _logger.LogInformation("Successfully processed Job {JobId}", job.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process Job {JobId}", job.Id);
            await processingService.FailJobAsync(job.Id, ex.Message, stoppingToken);
        }
    }
}
