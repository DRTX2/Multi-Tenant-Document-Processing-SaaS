using AspNetProject.Domain.Models;
using AspNetProject.Domain.ValueObjects;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetProject.Domain.Ports.In;

/// <summary>
/// Puerto de ENTRADA (Inbound Port) - Define la lógica de negocio para gestión de tenants
/// Responsable de:
/// - CRUD de tenants (creación, lectura, actualización, eliminación)
/// - Gestión de configuración de tenants
/// - Control de estados del tenant (activo, suspendido)
/// Este puerto es implementado por la capa de Application y usado por adaptadores de entrada (Adapters/In)
/// </summary>
public interface ITenantService
{
    /// <summary>
    /// Obtiene todos los tenants del sistema
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Colección de todos los tenants</returns>
    Task<IEnumerable<Tenant>> GetTenantsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtiene un tenant específico por su ID
    /// </summary>
    /// <param name="tenantId">Identificador del tenant</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>El tenant solicitado</returns>
    /// <exception cref="KeyNotFoundException">Cuando el tenant no existe</exception>
    Task<Tenant> GetTenantByIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtiene la configuración de un tenant específico
    /// Más ligero que obtener toda la entidad si solo se necesita la configuración
    /// </summary>
    /// <param name="tenantId">Identificador del tenant</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>La configuración del tenant</returns>
    /// <exception cref="KeyNotFoundException">Cuando el tenant no existe</exception>
    Task<TenantConfiguration> GetTenantConfigurationAsync(Guid tenantId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Crea un nuevo tenant en el sistema
    /// </summary>
    /// <param name="name">Nombre único del tenant</param>
    /// <param name="configuration">Configuración inicial del tenant</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>El tenant creado con su ID asignado</returns>
    /// <exception cref="ArgumentNullException">Cuando name o configuration son nulos</exception>
    /// <exception cref="InvalidOperationException">Cuando el nombre ya existe</exception>
    Task<Tenant> CreateTenantAsync(string name, TenantConfiguration configuration, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Actualiza la configuración de un tenant existente
    /// </summary>
    /// <param name="tenantId">Identificador del tenant</param>
    /// <param name="newConfiguration">Nueva configuración</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>El tenant con configuración actualizada</returns>
    /// <exception cref="KeyNotFoundException">Cuando el tenant no existe</exception>
    /// <exception cref="InvalidOperationException">Cuando el tenant está suspendido</exception>
    Task<Tenant> UpdateTenantConfigurationAsync(Guid tenantId, TenantConfiguration newConfiguration, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Suspende un tenant impidiendo que sus usuarios puedan acceder
    /// El tenant puede ser reactivado posteriormente
    /// </summary>
    /// <param name="tenantId">Identificador del tenant a suspender</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>El tenant suspendido</returns>
    /// <exception cref="KeyNotFoundException">Cuando el tenant no existe</exception>
    /// <exception cref="InvalidOperationException">Cuando el tenant ya está suspendido</exception>
    Task<Tenant> SuspendTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Reactiva un tenant previamente suspendido
    /// </summary>
    /// <param name="tenantId">Identificador del tenant a activar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>El tenant activado</returns>
    /// <exception cref="KeyNotFoundException">Cuando el tenant no existe</exception>
    /// <exception cref="InvalidOperationException">Cuando el tenant no está suspendido</exception>
    Task<Tenant> ActivateTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Elimina un tenant del sistema de forma permanente
    /// Esto eliminará todos los datos asociados (usuarios, documentos, etc.)
    /// Esta operación debe ser irreversible
    /// </summary>
    /// <param name="tenantId">Identificador del tenant a eliminar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <exception cref="KeyNotFoundException">Cuando el tenant no existe</exception>
    /// <exception cref="InvalidOperationException">Cuando el tenant tiene usuarios activos o documentos no procesados</exception>
    Task DeleteTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
}