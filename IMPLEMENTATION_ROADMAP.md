# 🚀 PLAN DE IMPLEMENTACIÓN - PRÓXIMOS PASOS

**Fecha de inicio:** 3 de Enero, 2026  
**Estado actual:** Puertos de entrada refactorizados ✅  
**Próxima fase:** Implementación de servicios en capa Application

---

## 📋 CHECKLIST DE COMPLETITUD

### Fase 1: Refactorización de Puertos ✅ COMPLETADA

- [x] Refactorizar IDocumentService
  - [x] Cambiar a async/await
  - [x] Usar DocumentMetadata en lugar de Dictionary
  - [x] Agregar ownerUserId para trazabilidad
  - [x] Nuevo método GetDocumentVersionsAsync()
  - [x] Documentación XML completa

- [x] Refactorizar IDocumentProcessingService
  - [x] Cambiar a async/await
  - [x] Cambiar retorno de string a ProcessingStatus enum
  - [x] Nuevo método GetDocumentProcessingJobAsync()
  - [x] Validaciones de estado documentadas
  - [x] Documentación XML completa

- [x] Refactorizar IUserService
  - [x] Cambiar roles de string a UserRole enum
  - [x] Agregar tenantId a GetUserByEmailAsync()
  - [x] Nuevo método GetUserByIdAsync()
  - [x] Nuevo método GetUsersByTenantIdAsync()
  - [x] Nuevo método GetUserRolesAsync()
  - [x] Documentación XML completa

- [x] Refactorizar IAuditService
  - [x] Cambiar a async/await
  - [x] Agregar ipAddress parameter (crucial)
  - [x] Agregar resource parameter
  - [x] Nuevo método GetAuditRecordsByResourceAsync()
  - [x] Nuevo método GetAuditRecordsByActionAsync()
  - [x] Documentación XML completa

- [x] Refactorizar ITenantService
  - [x] Cambiar a async/await
  - [x] Métodos de modificación retornan entidades
  - [x] Nuevo método GetTenantConfigurationAsync()
  - [x] Documentación XML completa

- [x] Actualizar IWeatherForecastService
  - [x] Cambiar a async/await
  - [x] Documentación XML mejorada

- [x] Actualizar IReportService
  - [x] Documentación XML mejorada (ya estaba bien)

---

## 🔧 Fase 2: Implementación de Servicios en Application Layer

### 2.1 DocumentService Implementation
**Archivo:** `/AspNetProject/Application/Services/DocumentService.cs`

```csharp
public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IDocumentVersionRepository _versionRepository;
    
    public async Task<IEnumerable<Document>> GetDocumentsByTenantIdAsync(
        Guid tenantId, 
        CancellationToken cancellationToken = default)
    {
        // ✅ Usar Repository para obtener documentos
        // ✅ Filtrar por TenantId
        // ✅ No cargar todas las versiones (performance)
        // ✅ Ignorar documentos marcados como DELETED
    }
    
    public async Task<Document> UploadDocumentAsync(
        Guid tenantId, 
        Guid ownerUserId, 
        DocumentMetadata metadata, 
        byte[] content, 
        CancellationToken cancellationToken = default)
    {
        // ✅ Validar que metadata no sea null
        // ✅ Crear entidad Document
        // ✅ Guardar contenido binario en almacenamiento (Storage/Blob)
        // ✅ Crear versión inicial
        // ✅ Guardar en repositorio
        // ✅ Registrar en auditoría (via IAuditService)
        // ✅ Retornar documento creado
    }
}
```

**Puntos clave:**
- Validar parámetros según documentación de puertos
- Usar repositories para acceso a datos
- Registrar acciones en auditoría
- Manejar excepciones de forma explícita

### 2.2 DocumentProcessingService Implementation
**Archivo:** `/AspNetProject/Application/Services/DocumentProcessingService.cs`

```csharp
public class DocumentProcessingService : IDocumentProcessingService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IDocumentProcessingJobRepository _jobRepository;
    private readonly IDocumentProcessingPublisher _eventPublisher; // Para mensajería
    
    public async Task<Guid> EnqueueDocumentForProcessingAsync(
        Guid documentId, 
        CancellationToken cancellationToken = default)
    {
        // ✅ Verificar que documento existe (throw KeyNotFoundException)
        // ✅ Verificar que documento está en estado UPLOADED (throw InvalidOperationException)
        // ✅ Crear DocumentProcessingJob en estado QUEUED
        // ✅ Cambiar estado del documento a QUEUED
        // ✅ Publicar evento para que worker inicie procesamiento
        // ✅ Retornar ID del job
    }
    
    public async Task<ProcessingStatus> GetDocumentProcessingStatusAsync(
        Guid documentId, 
        CancellationToken cancellationToken = default)
    {
        // ✅ Obtener el DocumentProcessingJob más reciente
        // ✅ Retornar su Status (enum ProcessingStatus)
        // ✅ Throw KeyNotFoundException si no existe
    }
}
```

**Puntos clave:**
- Usar `ProcessingStatus` enum, no strings
- Validar estados de documentos
- Implementar patrón de reintentos
- Publicar eventos para workers asincronos

### 2.3 UserService Implementation
**Archivo:** `/AspNetProject/Application/Services/UserService.cs`

```csharp
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    
    public async Task<TenantUser> CreateUserAsync(
        string email, 
        string password, 
        Guid tenantId, 
        UserRole[]? initialRoles = null, 
        CancellationToken cancellationToken = default)
    {
        // ✅ Validar email y password no son nulos
        // ✅ Verificar email único en tenant (throw InvalidOperationException)
        // ✅ Hash la contraseña (nunca almacenar plain text)
        // ✅ Crear TenantUser con roles iniciales (default: USER)
        // ✅ Guardar en repositorio
        // ✅ Registrar en auditoría
        // ✅ Retornar usuario creado
    }
    
    public async Task AssignRoleToUserAsync(
        Guid userId, 
        UserRole role, 
        CancellationToken cancellationToken = default)
    {
        // ✅ Verificar usuario existe (throw KeyNotFoundException)
        // ✅ Verificar que ya no tiene el rol (throw InvalidOperationException)
        // ✅ Agregar rol a colección _roles
        // ✅ Guardar cambios
        // ✅ Registrar en auditoría
    }
}
```

**Puntos clave:**
- **NUNCA** almacenar contraseñas en plain text
- Usar enum `UserRole` para roles
- Validar unicidad de email por tenant
- Usar IPasswordHasher para hash seguro

### 2.4 AuditService Implementation
**Archivo:** `/AspNetProject/Application/Services/AuditService.cs`

```csharp
public class AuditService : IAuditService
{
    private readonly IAuditLogRepository _auditRepository;
    
    public async Task<AuditLog> LogUserActionAsync(
        Guid tenantId, 
        string action, 
        string resource, 
        string ipAddress, 
        Guid? userId = null, 
        CancellationToken cancellationToken = default)
    {
        // ✅ Crear AuditLog con timestamp UTC actual
        // ✅ Usar ipAddress para trazabilidad
        // ✅ Guardar en repositorio
        // ✅ Retornar registro creado
        // ✅ Usar valores válidos: action=CREATE/UPDATE/DELETE, resource=Document/Tenant/User
    }
    
    public async Task<IEnumerable<AuditLog>> GetAuditRecordsByResourceAsync(
        string resource, 
        Guid tenantId, 
        CancellationToken cancellationToken = default)
    {
        // ✅ Usar LINQ para filtrar por resource y tenantId
        // ✅ Retornar IEnumerable (lazy evaluation)
    }
}
```

**Puntos clave:**
- Timestamps SIEMPRE en UTC
- ipAddress es crítico para trazabilidad
- Usar enumeraciones para action y resource (considerar enums)
- No modificar registros de auditoría (inmutables)

### 2.5 TenantService Implementation
**Archivo:** `/AspNetProject/Application/Services/TenantService.cs`

```csharp
public class TenantService : ITenantService
{
    private readonly ITenantRepository _tenantRepository;
    
    public async Task<Tenant> CreateTenantAsync(
        string name, 
        TenantConfiguration configuration, 
        CancellationToken cancellationToken = default)
    {
        // ✅ Validar name no es nulo/vacío
        // ✅ Verificar name único (throw InvalidOperationException)
        // ✅ Crear Tenant con status = ACTIVE
        // ✅ Guardar en repositorio
        // ✅ Registrar en auditoría
        // ✅ Retornar tenant creado
    }
    
    public async Task<Tenant> SuspendTenantAsync(
        Guid tenantId, 
        CancellationToken cancellationToken = default)
    {
        // ✅ Obtener tenant (throw KeyNotFoundException si no existe)
        // ✅ Verificar no está ya suspendido (throw InvalidOperationException)
        // ✅ Cambiar status a SUSPENDED
        // ✅ Guardar cambios
        // ✅ Registrar en auditoría
        // ✅ Retornar tenant modificado
    }
}
```

**Puntos clave:**
- Validar estados antes de modificar
- Nombre de tenant debe ser único en el sistema
- Documentar qué sucede cuando se suspende (usuarios no pueden acceder)

---

## 🔄 Fase 3: Actualizar Adaptadores (Controllers)

### Controllers Impactados:
```
/Adapters/In/Controllers/
├── DocumentController.cs
├── DocumentProcessingController.cs
├── UserController.cs
├── AuditController.cs
├── TenantController.cs
└── WeatherForecastController.cs
```

**Cambios necesarios:**
- Cambiar llamadas síncronas a async
- Actualizar `await` para nuevas firmas async
- Manejar `CancellationToken` desde HttpContext
- Actualizar Response DTOs si es necesario

---

## 🗄️ Fase 4: Repositories Secundarios

Verificar que existan interfaces de repositories:

```csharp
// Puertos de SALIDA (Out/Secondary Ports)
/Domain/Ports/Out/
├── IDocumentRepository.cs
├── IDocumentVersionRepository.cs
├── IDocumentProcessingJobRepository.cs
├── IUserRepository.cs
├── IPasswordHasher.cs  // Adapter
├── IAuditLogRepository.cs
└── ITenantRepository.cs
```

**Action:** Crear estos puertos si no existen, con métodos async que coincidan.

---

## 📊 Fase 5: Testing

### Escribir Tests Unitarios:

**Archivo:** `/AspNetProject.Tests/Application/Services/DocumentServiceTests.cs`

```csharp
[TestClass]
public class DocumentServiceTests
{
    [TestMethod]
    public async Task UploadDocumentAsync_WithValidData_ShouldCreateDocument()
    {
        // Arrange
        var documentService = new DocumentService(mockRepository);
        var metadata = new DocumentMetadata("test.pdf", "application/pdf", 1024);
        
        // Act
        var result = await documentService.UploadDocumentAsync(
            tenantId, ownerUserId, metadata, content, CancellationToken.None);
        
        // Assert
        Assert.IsNotNull(result.Id);
        Assert.AreEqual(DocumentStatus.UPLOADED, result.Status);
    }
    
    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public async Task UploadDocumentAsync_WithNullMetadata_ShouldThrow()
    {
        // Arrange
        var documentService = new DocumentService(mockRepository);
        
        // Act
        await documentService.UploadDocumentAsync(tenantId, ownerUserId, null, content, CancellationToken.None);
    }
}
```

---

## 🎯 PRIORIDAD DE IMPLEMENTACIÓN

1. **CRÍTICA - Semana 1:**
   - [ ] DocumentService + DocumentProcessingService
   - [ ] UserService
   - [ ] Repositories para estos servicios

2. **ALTA - Semana 2:**
   - [ ] AuditService
   - [ ] TenantService
   - [ ] Controllers actualizados

3. **MEDIA - Semana 3:**
   - [ ] Tests unitarios
   - [ ] Integration tests
   - [ ] Documentación de API

4. **BAJA - Semana 4:**
   - [ ] Performance optimization
   - [ ] Caching strategy
   - [ ] Monitoring/Telemetry

---

## 📝 NOTAS IMPORTANTES PARA IMPLEMENTACIÓN

### 1. Manejo de Errores
```csharp
// ✅ CORRECTO: Excepciones específicas documentadas
try
{
    var document = await _repository.GetDocumentByIdAsync(documentId);
    if (document == null)
        throw new KeyNotFoundException($"Documento {documentId} no encontrado");
}
catch (KeyNotFoundException)
{
    // Manejar en controller
}
```

### 2. Validación de Estados
```csharp
// ✅ CORRECTO: Validar estado antes de operación
if (document.Status != DocumentStatus.UPLOADED)
    throw new InvalidOperationException(
        $"No se puede procesar documento en estado {document.Status}");
```

### 3. Registrar en Auditoría
```csharp
// ✅ CORRECTO: Auditar operaciones críticas
await _auditService.LogUserActionAsync(
    tenantId: tenantId,
    action: "CREATE",
    resource: "Document",
    ipAddress: GetClientIpAddress(), // Del HttpContext
    userId: currentUserId);
```

### 4. Usar CancellationToken
```csharp
// ✅ CORRECTO: Pasar token a operaciones async
await _repository.SaveAsync(entity, cancellationToken);
```

---

## ✅ CHECKLIST FINAL ANTES DE MERGE

- [ ] Todos los puertos tienen async/await
- [ ] Todos los métodos async incluyen CancellationToken
- [ ] Value objects usados en lugar de Dictionary/strings
- [ ] Documentación XML en cada método
- [ ] Excepciones documentadas
- [ ] Sin errores de compilación
- [ ] Tests unitarios escritos
- [ ] Controllers actualizados
- [ ] Repositories creados
- [ ] Base de datos migrada (si es necesario)

---

## 🚀 PRÓXIMA ACCIÓN RECOMENDADA

**Crear archivo:** `IMPLEMENTATION_GUIDE.md` con:
- Estructura específica de cada servicio
- Inyección de dependencias
- Configuración en Program.cs
- DTOs para endpoints REST

**Tiempo estimado:** 2-3 semanas para implementación completa

---

**Estado:** ✅ Plan de implementación listo  
**Bloqueadores:** Ninguno  
**Próxima reunión:** Kickoff de implementación de servicios

