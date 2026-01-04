using AspNetProject.Domain.Models;
using AspNetProject.Domain.Ports.In;
using AspNetProject.Domain.Ports.Out;
using AspNetProject.Domain.ValueObjects;

namespace AspNetProject.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    // In a real scenario, inject a separate IPasswordHasher service
    
    public UserService(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<TenantUser> GetUserByEmailAsync(string email, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _userRepository.GetByEmailAsync(email, tenantId, cancellationToken)
               ?? throw new KeyNotFoundException($"User with email {email} not found for tenant {tenantId}");
    }

    public async Task<TenantUser> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
         return await _userRepository.GetByIdAsync(userId, cancellationToken)
               ?? throw new KeyNotFoundException($"User with ID {userId} not found");
    }

    public async Task<IEnumerable<TenantUser>> GetUsersByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _userRepository.GetByTenantIdAsync(tenantId, cancellationToken);
    }

    public async Task<TenantUser> CreateUserAsync(string email, string password, Guid tenantId, UserRole[]? initialRoles = null,
        CancellationToken cancellationToken = default)
    {
        var existing = await _userRepository.GetByEmailAsync(email, tenantId, cancellationToken);
        if (existing != null)
        {
            throw new InvalidOperationException($"User with email {email} already exists in this tenant.");
        }

        // TODO: Hash password properly using a secure hasher service
        // For MVP, we'll store pseudo-hashed or defer this to Infrastructure implementation detail (bad practice in domain)
        // Correct way: Inject IPasswordHasher into UserService. 
        // string hashedPassword = _passwordHasher.Hash(password);
        
        // Mock hashing for code completeness:
        string hashedPassword = $"hashed_{password}"; 

        var user = new TenantUser(tenantId, email, hashedPassword);
        
        if (initialRoles != null)
        {
            foreach (var role in initialRoles)
            {
               user.AddRole(role);
            }
        }
        else
        {
            user.AddRole(UserRole.USER);
        }

        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return user;
    }

    public async Task<TenantUser> UpdateUserAsync(Guid userId, string? email = null, string? password = null, CancellationToken cancellationToken = default)
    {
        var user = await GetUserByIdAsync(userId, cancellationToken);

        if (!string.IsNullOrEmpty(email))
        {
            // Check uniqueness if email changed
            if (email != user.Email)
            {
                var existing = await _userRepository.GetByEmailAsync(email, user.TenantId, cancellationToken);
                if (existing != null)
                     throw new InvalidOperationException($"Email {email} is already taken.");
                
                user.UpdateEmail(email);
            }
        }

        if (!string.IsNullOrEmpty(password))
        {
             // Mock hashing
             string hashedPassword = $"hashed_{password}";
             user.UpdatePassword(hashedPassword);
        }

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return user;
    }

    public async Task AssignRoleToUserAsync(Guid userId, UserRole role, CancellationToken cancellationToken = default)
    {
        var user = await GetUserByIdAsync(userId, cancellationToken);
        
        try 
        {
            user.AddRole(role);
        }
        catch (InvalidOperationException)
        {
             // Rethrow or handle specific logic if already has role
             throw; 
        }

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveRoleFromUserAsync(Guid userId, UserRole role, CancellationToken cancellationToken = default)
    {
        var user = await GetUserByIdAsync(userId, cancellationToken);
        
        user.RemoveRole(role);

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<UserRole>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
         var user = await GetUserByIdAsync(userId, cancellationToken);
         return user.Roles; 
    }

    public async Task LockUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await GetUserByIdAsync(userId, cancellationToken);
        
        user.Lock(); 
        
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task UnlockUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await GetUserByIdAsync(userId, cancellationToken);
        
        user.Unlock();
        
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await GetUserByIdAsync(userId, cancellationToken);
        
        // Hard delete or Soft delete? Domain model usually dictates.
        // Assuming repository DeleteAsync handles it (EF Core Remove -> Physical delete).
        await _userRepository.DeleteAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
