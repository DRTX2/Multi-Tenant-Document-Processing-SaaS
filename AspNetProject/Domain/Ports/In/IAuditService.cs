using AspNetProject.Domain.Models;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetProject.Domain.Ports.In;

/// <summary>
/// Puerto de ENTRADA (Inbound Port) - Define la lógica de negocio para auditoría
/// Responsable de:
/// - Registrar acciones de usuarios (logs de auditoría)
/// - Recuperar registros de auditoría con filtros avanzados
/// - Proporcionar trazabilidad completa de operaciones
/// Este puerto es implementado por la capa de Application y usado por adaptadores de entrada (Adapters/In)
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Registra una acción de usuario en el sistema de auditoría
    /// Crea un registro inmutable con timestamp de UTC
    /// </summary>
    /// <param name="tenantId">Identificador del tenant donde ocurrió la acción</param>
    /// <param name="userId">Identificador del usuario que realizó la acción (opcional)</param>
    /// <param name="action">Descripción de la acción realizada (ej: "CREATE", "UPDATE", "DELETE")</param>
    /// <param name="resource">Tipo de recurso afectado (ej: "Document", "Tenant", "User")</param>
    /// <param name="ipAddress">Dirección IP desde donde se realizó la acción</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>El registro de auditoría creado</returns>
    /// <exception cref="ArgumentNullException">Cuando algún parámetro requerido es nulo</exception>
    Task<AuditLog> LogUserActionAsync(
        Guid tenantId,
        string action,
        string resource,
        string ipAddress,
        Guid? userId = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtiene registros de auditoría con filtros opcionales
    /// Todos los filtros son opcionales - omitir devuelve todos los registros
    /// </summary>
    /// <param name="tenantId">Filtro por tenant (opcional)</param>
    /// <param name="userId">Filtro por usuario (opcional)</param>
    /// <param name="startDate">Filtro por fecha inicio (inclusive, UTC)</param>
    /// <param name="endDate">Filtro por fecha fin (inclusive, UTC)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Colección de registros de auditoría que coinciden con los filtros</returns>
    Task<IEnumerable<AuditLog>> GetAuditRecordsAsync(
        Guid? tenantId = null,
        Guid? userId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtiene registros de auditoría filtrados por recurso específico
    /// </summary>
    /// <param name="resource">Tipo de recurso (ej: "Document", "Tenant")</param>
    /// <param name="tenantId">Filtro por tenant</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Colección de registros relacionados con el recurso</returns>
    /// <exception cref="ArgumentNullException">Cuando resource es nulo</exception>
    Task<IEnumerable<AuditLog>> GetAuditRecordsByResourceAsync(
        string resource,
        Guid tenantId,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtiene registros de auditoría filtrados por acción específica
    /// </summary>
    /// <param name="action">Tipo de acción (ej: "CREATE", "UPDATE", "DELETE")</param>
    /// <param name="tenantId">Filtro por tenant</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Colección de registros con la acción especificada</returns>
    /// <exception cref="ArgumentNullException">Cuando action es nulo</exception>
    Task<IEnumerable<AuditLog>> GetAuditRecordsByActionAsync(
        string action,
        Guid tenantId,
        CancellationToken cancellationToken = default);
}