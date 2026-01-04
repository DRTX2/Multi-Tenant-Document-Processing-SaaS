# 📦 Infrastructure Layer - README

**Propósito:** Capa de infraestructura que implementa los puertos de salida (Out Ports)  
**Responsabilidades:**
- Persistencia de datos (EF Core + Repositories)
- Almacenamiento de archivos (File System / Azure)
- Caché distribuida (Redis)
- Hashing de contraseñas (BCrypt)
- Procesamiento en background (Hangfire)

---

## 🗂️ ESTRUCTURA

```
Infrastructure/
├── README.md (este archivo)
├── Persistence/
│   ├── README.md
│   ├── ApplicationDbContext.cs
│   ├── Configurations/
│   │   ├── DocumentConfiguration.cs
│   │   ├── DocumentVersionConfiguration.cs
│   │   ├── TenantUserConfiguration.cs
│   │   ├── AuditLogConfiguration.cs
│   │   ├── TenantConfiguration.cs
│   │   └── DocumentProcessingJobConfiguration.cs
│   └── Repositories/
│       ├── GenericRepository.cs
│       ├── DocumentRepository.cs
│       ├── DocumentVersionRepository.cs
│       ├── DocumentProcessingJobRepository.cs
│       ├── UserRepository.cs
│       ├── AuditLogRepository.cs
│       └── TenantRepository.cs
│
├── Storage/
│   ├── README.md
│   ├── FileSystemDocumentStorage.cs
│   ├── AzureBlobStorageDocumentStorage.cs
│   └── StorageConstants.cs
│
├── Providers/
│   ├── README.md
│   ├── BcryptPasswordHasher.cs
│   ├── RedisCacheProvider.cs
│   ├── HangfireBackgroundJobProcessor.cs
│   └── Constants/
│       └── CacheKeyConstants.cs
│
└── Extensions/
    ├── ServiceCollectionExtensions.cs
    └── ServiceExtensions.cs
```

---

## 📖 SUBCARPETAS

### Persistence/
Implementación de los 7 repositorios usando EF Core:
- `ApplicationDbContext` - DbContext principal
- `Configurations/` - Entity Type Configurations
- `Repositories/` - Implementación de interfaces

**Dependencias:**
- `Microsoft.EntityFrameworkCore`
- `Microsoft.EntityFrameworkCore.SqlServer`

### Storage/
Implementaciones de almacenamiento de archivos:
- FileSystemDocumentStorage - Para desarrollo
- AzureBlobStorageDocumentStorage - Para producción

**Dependencias:**
- `Azure.Storage.Blobs` (opcional, para Azure)

### Providers/
Implementaciones de servicios especializados:
- BcryptPasswordHasher - Hashing seguro
- RedisCacheProvider - Caché distribuida
- HangfireBackgroundJobProcessor - Jobs en background

**Dependencias:**
- `BCrypt.Net-Next`
- `StackExchange.Redis`
- `Hangfire`
- `Hangfire.SqlServer`

### Extensions/
Métodos de extensión para inyección de dependencias:
- ServiceCollectionExtensions - AddXxx methods

---

## 🔧 CÓMO USAR

### Registrar Servicios (Program.cs)

```csharp
builder.Services
    .AddRepositories(configuration)
    .AddStorageProvider(configuration)
    .AddCacheProvider(configuration)
    .AddPasswordHasher()
    .AddBackgroundJobProcessor(configuration);
```

### Inyectar en Servicios

```csharp
public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IDocumentStorage _storage;
    private readonly IBackgroundJobProcessor _jobProcessor;
    
    public DocumentService(
        IDocumentRepository documentRepository,
        IDocumentStorage storage,
        IBackgroundJobProcessor jobProcessor)
    {
        _documentRepository = documentRepository;
        _storage = storage;
        _jobProcessor = jobProcessor;
    }
}
```

---

## ⚙️ CONFIGURACIÓN

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=AspNetProjectDb;Trusted_Connection=true;Encrypt=false;"
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  },
  "Storage": {
    "Provider": "FileSystem", // "FileSystem" o "AzureBlob"
    "FileSystemPath": "./uploads",
    "AzureBlobConnection": "DefaultEndpointsProtocol=https;..."
  },
  "Hangfire": {
    "DashboardPath": "/hangfire",
    "WorkerCount": 4
  }
}
```

---

## 🚀 FLUJO DE DATOS TÍPICO

### Crear Documento
```
Controller
  ↓
IDocumentService.UploadDocumentAsync()
  ↓
DocumentService (Application)
  ├─ IDocumentRepository.CreateAsync() → DB
  ├─ IDocumentStorage.SaveAsync() → File/Azure
  ├─ IAuditLogRepository.CreateAsync() → Auditoría
  └─ ICacheProvider.RemoveAsync() → Invalida caché
  ↓
Retorna Document
```

### Procesar Documento
```
Controller
  ↓
IDocumentProcessingService.EnqueueDocumentForProcessingAsync()
  ↓
DocumentProcessingService (Application)
  ├─ IDocumentProcessingJobRepository.CreateAsync()
  ├─ IBackgroundJobProcessor.EnqueueOcrProcessingAsync()
  └─ IAuditLogRepository.CreateAsync()
  ↓
Hangfire Worker (Background)
  ├─ Obtiene IDocumentStorage
  ├─ Procesa contenido
  └─ Actualiza estado
```

---

## 📊 MULTI-TENANCY

Todos los repositorios incluyen validación de tenant:

```csharp
// ✅ CORRECTO: Incluye validación de tenant
public async Task<Document?> GetByIdAsync(Guid documentId, Guid tenantId, CancellationToken ct)
{
    return await _dbContext.Documents
        .Where(d => d.Id == documentId && d.TenantId == tenantId) // ← Validación
        .FirstOrDefaultAsync(ct);
}

// ❌ INCORRECTO: Sin validación
public async Task<Document?> GetByIdAsync(Guid documentId, CancellationToken ct)
{
    return await _dbContext.Documents
        .FirstOrDefaultAsync(d => d.Id == documentId, ct);
}
```

---

## 🔒 SEGURIDAD

### Passwords
```csharp
// Nunca plain text
string hash = _passwordHasher.Hash(plainPassword);
bool valid = _passwordHasher.Verify(plainPassword, hash);
```

### Auditoría
```csharp
// Immutable (APPEND-ONLY)
await _auditRepository.CreateAsync(new AuditLog(
    tenantId,
    action: "CREATE",
    resource: "Document",
    ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString()
));
```

### Storage
```csharp
// URLs temporales
string url = await _storage.GetTemporaryUrlAsync(
    documentId, tenantId, expirationMinutes: 15);
```

---

## ⚡ PERFORMANCE

### Índices
```sql
CREATE INDEX IX_Document_TenantId_Status ON Documents(TenantId, Status);
CREATE INDEX IX_TenantUser_TenantId_Email ON TenantUsers(TenantId, Email);
CREATE INDEX IX_AuditLog_TenantId_OccurredAt ON AuditLogs(TenantId, OccurredAt DESC);
```

### Caché
```csharp
// Pattern cache-aside
var user = await _cache.GetOrCreateAsync(
    key: $"user:{userId}",
    factory: async (ct) => await _userRepository.GetByIdAsync(userId, tenantId, ct),
    expiration: TimeSpan.FromHours(1)
);
```

### Batch Operations
```csharp
// Crear múltiples en una transacción
await _auditRepository.CreateBatchAsync(auditLogs);
```

---

## 🧪 TESTING

### Unit Tests con InMemory Database
```csharp
var options = new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseInMemoryDatabase("TestDb")
    .Options;

using var context = new ApplicationDbContext(options);
var repository = new DocumentRepository(context);
```

### Integration Tests con SQL Server Real
```csharp
// Usar TestContainer de SQL Server o SQL Express
var connectionString = "Server=.;Database=AspNetProjectTest;...";
```

---

## 📚 PRÓXIMOS DOCUMENTOS

- `Persistence/README.md` - Guía de persistencia
- `Storage/README.md` - Guía de almacenamiento
- `Providers/README.md` - Guía de providers
- `INFRASTRUCTURE_SETUP.md` - Pasos de setup
- `DATABASE_SCHEMA.md` - Esquema SQL

---

**Generado por:** GitHub Copilot  
**Fecha:** 3 de Enero, 2026  
**Estado:** Plan + Estructura Base Lista

