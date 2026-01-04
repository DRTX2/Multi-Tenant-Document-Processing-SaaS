using AspNetProject.Domain.Models;
using AspNetProject.Domain.ValueObjects;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetProject.Domain.Ports.In;

/// <summary>
/// Puerto de ENTRADA (Inbound Port) - Define la lógica de negocio para gestión de documentos
/// Responsable de:
/// - Subida y almacenamiento de documentos
/// - Gestión de versiones de documentos
/// - Control de soft delete y restauración
/// - Actualización de metadatos
/// Este puerto es implementado por la capa de Application y usado por adaptadores de entrada (Adapters/In)
/// </summary>
public interface IDocumentService
{
    /// <summary>
    /// Obtiene todos los documentos de un tenant específico
    /// </summary>
    /// <param name="tenantId">Identificador del tenant propietario de los documentos</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Colección de documentos del tenant</returns>
    Task<IEnumerable<Document>> GetDocumentsByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtiene un documento específico por su ID
    /// </summary>
    /// <param name="documentId">Identificador del documento</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>El documento solicitado</returns>
    /// <exception cref="KeyNotFoundException">Cuando el documento no existe</exception>
    Task<Document> GetDocumentByIdAsync(Guid documentId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Sube un nuevo documento al sistema
    /// </summary>
    /// <param name="tenantId">Identificador del tenant propietario</param>
    /// <param name="ownerUserId">Identificador del usuario que sube el documento</param>
    /// <param name="metadata">Metadatos del documento (nombre, tipo contenido, tamaño)</param>
    /// <param name="content">Contenido binario del documento</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>El documento creado con ID asignado</returns>
    /// <exception cref="ArgumentNullException">Cuando metadata es nulo</exception>
    Task<Document> UploadDocumentAsync(Guid tenantId, Guid ownerUserId, DocumentMetadata metadata, byte[] content, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtiene todas las versiones de un documento específico
    /// </summary>
    /// <param name="documentId">Identificador del documento</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Colección de versiones del documento</returns>
    Task<IEnumerable<DocumentVersion>> GetDocumentVersionsAsync(Guid documentId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Actualiza los metadatos de un documento existente
    /// </summary>
    /// <param name="documentId">Identificador del documento a actualizar</param>
    /// <param name="newMetadata">Nuevos metadatos del documento</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>El documento con metadatos actualizados</returns>
    /// <exception cref="KeyNotFoundException">Cuando el documento no existe</exception>
    Task<Document> UpdateDocumentMetadataAsync(Guid documentId, DocumentMetadata newMetadata, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Marca un documento como eliminado (soft delete)
    /// El documento no se elimina físicamente de la base de datos
    /// </summary>
    /// <param name="documentId">Identificador del documento a eliminar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <exception cref="KeyNotFoundException">Cuando el documento no existe</exception>
    Task SoftDeleteDocumentAsync(Guid documentId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Restaura un documento previamente marcado como eliminado
    /// </summary>
    /// <param name="documentId">Identificador del documento a restaurar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>El documento restaurado</returns>
    /// <exception cref="KeyNotFoundException">Cuando el documento no existe</exception>
    Task<Document> RestoreDocumentAsync(Guid documentId, CancellationToken cancellationToken = default);
}