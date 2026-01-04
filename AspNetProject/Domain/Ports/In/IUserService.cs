using AspNetProject.Domain.Models;
using AspNetProject.Domain.ValueObjects;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetProject.Domain.Ports.In;

/// <summary>
/// Puerto de ENTRADA (Inbound Port) - Define la lógica de negocio para gestión de usuarios
/// Responsable de:
/// - CRUD de usuarios por tenant
/// - Asignación y remoción de roles
/// - Control de estados (activo, bloqueado)
/// - Autenticación básica
/// Este puerto es implementado por la capa de Application y usado por adaptadores de entrada (Adapters/In)
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Obtiene un usuario por su email dentro de un tenant específico
    /// </summary>
    /// <param name="email">Dirección de correo electrónico del usuario</param>
    /// <param name="tenantId">Identificador del tenant</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>El usuario encontrado</returns>
    /// <exception cref="KeyNotFoundException">Cuando el usuario no existe</exception>
    Task<TenantUser> GetUserByEmailAsync(string email, Guid tenantId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtiene un usuario por su ID
    /// </summary>
    /// <param name="userId">Identificador del usuario</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>El usuario encontrado</returns>
    /// <exception cref="KeyNotFoundException">Cuando el usuario no existe</exception>
    Task<TenantUser> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtiene todos los usuarios de un tenant específico
    /// </summary>
    /// <param name="tenantId">Identificador del tenant</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Colección de usuarios del tenant</returns>
    Task<IEnumerable<TenantUser>> GetUsersByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Crea un nuevo usuario en el sistema
    /// </summary>
    /// <param name="email">Dirección de correo electrónico (único por tenant)</param>
    /// <param name="password">Contraseña del usuario (será hasheada internamente)</param>
    /// <param name="tenantId">Identificador del tenant propietario</param>
    /// <param name="initialRoles">Roles iniciales del usuario (por defecto USER)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>El usuario creado con su ID asignado</returns>
    /// <exception cref="ArgumentNullException">Cuando email o password son nulos</exception>
    /// <exception cref="InvalidOperationException">Cuando el email ya existe en el tenant</exception>
    Task<TenantUser> CreateUserAsync(
        string email,
        string password,
        Guid tenantId,
        UserRole[]? initialRoles = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Actualiza los datos de un usuario existente
    /// </summary>
    /// <param name="userId">Identificador del usuario a actualizar</param>
    /// <param name="email">Nuevo email (opcional, si se proporciona debe ser único)</param>
    /// <param name="password">Nueva contraseña (opcional, si se proporciona será hasheada)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>El usuario actualizado</returns>
    /// <exception cref="KeyNotFoundException">Cuando el usuario no existe</exception>
    /// <exception cref="InvalidOperationException">Cuando el nuevo email ya existe</exception>
    Task<TenantUser> UpdateUserAsync(
        Guid userId,
        string? email = null,
        string? password = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Asigna un rol a un usuario
    /// El usuario puede tener múltiples roles
    /// </summary>
    /// <param name="userId">Identificador del usuario</param>
    /// <param name="role">Rol a asignar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <exception cref="KeyNotFoundException">Cuando el usuario no existe</exception>
    /// <exception cref="InvalidOperationException">Cuando el usuario ya tiene el rol</exception>
    Task AssignRoleToUserAsync(Guid userId, UserRole role, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Remueve un rol de un usuario
    /// </summary>
    /// <param name="userId">Identificador del usuario</param>
    /// <param name="role">Rol a remover</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <exception cref="KeyNotFoundException">Cuando el usuario no existe</exception>
    /// <exception cref="InvalidOperationException">Cuando el usuario no tiene el rol</exception>
    Task RemoveRoleFromUserAsync(Guid userId, UserRole role, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtiene todos los roles de un usuario
    /// </summary>
    /// <param name="userId">Identificador del usuario</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Colección de roles del usuario</returns>
    /// <exception cref="KeyNotFoundException">Cuando el usuario no existe</exception>
    Task<IEnumerable<UserRole>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Bloquea un usuario impidiendo que inicie sesión
    /// </summary>
    /// <param name="userId">Identificador del usuario a bloquear</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <exception cref="KeyNotFoundException">Cuando el usuario no existe</exception>
    Task LockUserAsync(Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Desbloquea un usuario permitiendo que inicie sesión nuevamente
    /// </summary>
    /// <param name="userId">Identificador del usuario a desbloquear</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <exception cref="KeyNotFoundException">Cuando el usuario no existe</exception>
    Task UnlockUserAsync(Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Elimina un usuario del sistema
    /// </summary>
    /// <param name="userId">Identificador del usuario a eliminar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <exception cref="KeyNotFoundException">Cuando el usuario no existe</exception>
    Task DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default);
}