using AspNetProject.Domain.Models;
using AspNetProject.Domain.ValueObjects;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetProject.Domain.Ports.In;

/// <summary>
/// Puerto de ENTRADA (Inbound Port) - Define la lógica de negocio para procesamiento de documentos
/// Responsable de:
/// - Encolar documentos para procesamiento
/// - Obtener estado actual de procesamiento
/// - Iniciar operaciones de OCR, indexación y clasificación
/// Nota: Trabaja con DocumentProcessingJob para mantener historial y reintentos
/// Este puerto es implementado por la capa de Application y usado por adaptadores de entrada (Adapters/In)
/// </summary>
public interface IDocumentProcessingService
{
    /// <summary>
    /// Encola un documento para procesamiento
    /// Crea un DocumentProcessingJob en estado QUEUED
    /// </summary>
    /// <param name="documentId">Identificador del documento a procesar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>ID del trabajo de procesamiento creado</returns>
    /// <exception cref="KeyNotFoundException">Cuando el documento no existe</exception>
    /// <exception cref="InvalidOperationException">Cuando el documento no está en estado UPLOADED</exception>
    Task<Guid> EnqueueDocumentForProcessingAsync(Guid documentId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtiene el estado actual de procesamiento de un documento
    /// </summary>
    /// <param name="documentId">Identificador del documento</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Estado actual del procesamiento (QUEUED, PROCESSING, COMPLETED, FAILED)</returns>
    /// <exception cref="KeyNotFoundException">Cuando el documento no existe o no tiene trabajo de procesamiento</exception>
    Task<ProcessingStatus> GetDocumentProcessingStatusAsync(Guid documentId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtiene el trabajo de procesamiento completo de un documento
    /// </summary>
    /// <param name="documentId">Identificador del documento</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>La entidad DocumentProcessingJob con detalles completos</returns>
    /// <exception cref="KeyNotFoundException">Cuando el documento no existe o no tiene trabajo de procesamiento</exception>
    Task<DocumentProcessingJob> GetDocumentProcessingJobAsync(Guid documentId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Inicia el procesamiento OCR para un documento
    /// Cambia el estado del documento a PROCESSING
    /// </summary>
    /// <param name="documentId">Identificador del documento</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <exception cref="KeyNotFoundException">Cuando el documento no existe</exception>
    /// <exception cref="InvalidOperationException">Cuando el documento no está en estado apto para OCR</exception>
    Task StartOcrProcessingAsync(Guid documentId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Inicia la indexación de un documento
    /// Prepara el documento para búsquedas y recuperación
    /// </summary>
    /// <param name="documentId">Identificador del documento</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <exception cref="KeyNotFoundException">Cuando el documento no existe</exception>
    /// <exception cref="InvalidOperationException">Cuando el documento no está en estado apto para indexación</exception>
    Task StartIndexingAsync(Guid documentId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Inicia la clasificación automática de un documento
    /// Categoriza el documento según su contenido
    /// </summary>
    /// <param name="documentId">Identificador del documento</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <exception cref="KeyNotFoundException">Cuando el documento no existe</exception>
    /// <exception cref="InvalidOperationException">Cuando el documento no está en estado apto para clasificación</exception>
    Task StartClassificationAsync(Guid documentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marca un trabajo como completado exitosamente.
    /// Actualiza el estado del trabajo y del documento.
    /// </summary>
    Task CompleteJobAsync(Guid jobId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marca un trabajo como fallido.
    /// Registra el error y actualiza el estado.
    /// </summary>
    Task FailJobAsync(Guid jobId, string errorMessage, CancellationToken cancellationToken = default);
}