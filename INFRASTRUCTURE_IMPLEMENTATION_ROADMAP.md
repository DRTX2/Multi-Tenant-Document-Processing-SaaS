# 🏗️ FASE 3: IMPLEMENTACIÓN DE INFRAESTRUCTURA - ROADMAP DETALLADO

**Proyecto:** AspNetProject  
**Fase:** 3 - Infraestructura  
**Fecha Inicio:** 3 de Enero, 2026  
**Duración Estimada:** 3-4 semanas

---

## 📋 OVERVIEW

En esta fase implementaremos todas las interfaces de puertos de salida (Out Ports) con tecnologías concretas:
- ✅ **Repositorios:** EF Core + SQL Server
- ✅ **Storage:** File System (desarrollo) / Azure Blob (producción)
- ✅ **Caché:** Redis
- ✅ **Hash de Passwords:** BCrypt
- ✅ **Background Jobs:** Hangfire

---

## 🗂️ ESTRUCTURA DE INFRAESTRUCTURA

```
Infrastructure/
├── README.md                              [Guía general]
├── Persistence/
│   ├── README.md
│   ├── ApplicationDbContext.cs            [EF Core DbContext]
│   ├── Configurations/                    [Entity Type Configs]
│   │   ├── DocumentConfiguration.cs
│   │   ├── DocumentVersionConfiguration.cs
│   │   ├── TenantUserConfiguration.cs
│   │   ├── AuditLogConfiguration.cs
│   │   ├── TenantConfiguration.cs
│   │   └── DocumentProcessingJobConfiguration.cs
│   ├── Migrations/                        [EF Core Migrations]
│   │   └── Initial/
│   └── Repositories/                      [Implementaciones]
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
│   ├── FileSystemDocumentStorage.cs       [Desarrollo]
│   ├── AzureBlobStorageDocumentStorage.cs [Producción]
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
    ├── ServiceCollectionExtensions.cs     [Inyección de dependencias]
    └── ServiceExtensions.cs
```

---

## 📅 TIMELINE DETALLADO

### SEMANA 1: CONFIGURACIÓN Y REPOSITORIOS BASE

#### Día 1: Setup EF Core
- [ ] Crear ApplicationDbContext
- [ ] Configurar connection string
- [ ] Instalar paquetes NuGet:
  - `Microsoft.EntityFrameworkCore.SqlServer`
  - `Microsoft.EntityFrameworkCore.Tools`

**Archivos a crear:**
```
✓ Infrastructure/Persistence/ApplicationDbContext.cs
✓ Infrastructure/Persistence/Configurations/ (carpeta)
```

#### Día 2: Entity Configurations
- [ ] DocumentConfiguration (índices en TenantId, Status)
- [ ] TenantUserConfiguration (índices en TenantId, Email)
- [ ] AuditLogConfiguration (índices en TenantId, OccurredAt)
- [ ] TenantConfiguration (índice en Name)
- [ ] DocumentProcessingJobConfiguration (índices en Status, DocumentId)
- [ ] DocumentVersionConfiguration (índices en DocumentId)

**Archivos a crear:**
```
✓ 6 archivos de configuración en Configurations/
```

#### Día 3: Initial Migration
- [ ] Crear migration inicial
- [ ] Validar schema SQL
- [ ] Crear script de base de datos

**Comandos:**
```bash
dotnet ef migrations add Initial -p AspNetProject/AspNetProject.csproj
dotnet ef database update
```

#### Día 4-5: GenericRepository
- [ ] Implementar IGenericRepository<T>
- [ ] Métodos base: GetByIdAsync, FindAsync, CreateAsync, etc.
- [ ] Tests unitarios básicos

**Archivo a crear:**
```
✓ Infrastructure/Persistence/Repositories/GenericRepository.cs
```

### SEMANA 2: REPOSITORIOS ESPECIALIZADOS

#### Día 6: DocumentRepository
- [ ] Implementar IDocumentRepository
- [ ] GetByTenantIdAsync con paginación
- [ ] SearchByNameAsync con LIKE
- [ ] GetByStatusAsync
- [ ] Multi-tenancy validation

**Archivo a crear:**
```
✓ Infrastructure/Persistence/Repositories/DocumentRepository.cs
```

#### Día 7: UserRepository
- [ ] Implementar IUserRepository
- [ ] GetByEmailAsync con tenant validation
- [ ] EmailExistsAsync para unicidad
- [ ] GetByRoleAsync
- [ ] GetByStatusAsync

**Archivo a crear:**
```
✓ Infrastructure/Persistence/Repositories/UserRepository.cs
```

#### Día 8: AuditLogRepository
- [ ] Implementar IAuditLogRepository
- [ ] GetByFilterAsync flexible
- [ ] CreateBatchAsync optimizado
- [ ] GetSuspiciousActivityAsync

**Archivo a crear:**
```
✓ Infrastructure/Persistence/Repositories/AuditLogRepository.cs
```

#### Día 9: TenantRepository
- [ ] Implementar ITenantRepository
- [ ] GetByStatusAsync
- [ ] GetActiveTenantAsync
- [ ] GetStatisticsAsync con agregaciones

**Archivo a crear:**
```
✓ Infrastructure/Persistence/Repositories/TenantRepository.cs
```

#### Día 10: Repositorios de Documentos
- [ ] Implementar IDocumentVersionRepository
- [ ] Implementar IDocumentProcessingJobRepository
- [ ] GetStatisticsAsync para ambos
- [ ] GetStaledJobsAsync para jobs

**Archivos a crear:**
```
✓ DocumentVersionRepository.cs
✓ DocumentProcessingJobRepository.cs
```

### SEMANA 3: ALMACENAMIENTO Y PROVIDERS

#### Día 11: FileSystem Storage (Desarrollo)
- [ ] Implementar IDocumentStorage con File System
- [ ] SaveAsync - guardar en carpeta por tenant
- [ ] GetAsync - recuperar archivo
- [ ] DeleteAsync - eliminar archivo
- [ ] GetTenantStorageUsageAsync

**Archivo a crear:**
```
✓ Infrastructure/Storage/FileSystemDocumentStorage.cs
```

#### Día 12: Azure Blob Storage (Producción)
- [ ] Instalar `Azure.Storage.Blobs`
- [ ] Implementar IDocumentStorage con Azure
- [ ] Mismos métodos que FileSystem
- [ ] Configuración con connection string

**Archivo a crear:**
```
✓ Infrastructure/Storage/AzureBlobStorageDocumentStorage.cs
```

#### Día 13: Password Hasher
- [ ] Instalar `BCrypt.Net-Next`
- [ ] Implementar IPasswordHasher
- [ ] Hash() - generar hash seguro
- [ ] Verify() - verificación
- [ ] ValidatePasswordStrength()

**Archivo a crear:**
```
✓ Infrastructure/Providers/BcryptPasswordHasher.cs
```

#### Día 14: Cache Provider (Redis)
- [ ] Instalar `StackExchange.Redis`
- [ ] Implementar ICacheProvider
- [ ] GetAsync<T>, SetAsync<T>
- [ ] GetOrCreateAsync<T> con factory
- [ ] Manejo de expiración

**Archivo a crear:**
```
✓ Infrastructure/Providers/RedisCacheProvider.cs
```

#### Día 15: Background Job Processor (Hangfire)
- [ ] Instalar `Hangfire` y `Hangfire.SqlServer`
- [ ] Implementar IBackgroundJobProcessor
- [ ] EnqueueOcrProcessingAsync - encolar
- [ ] GetJobStatusAsync - estado
- [ ] CancelJobAsync - cancelar

**Archivo a crear:**
```
✓ Infrastructure/Providers/HangfireBackgroundJobProcessor.cs
```

### SEMANA 4: INYECCIÓN DE DEPENDENCIAS

#### Día 16: ServiceCollectionExtensions
- [ ] Crear método AddRepositories()
- [ ] Crear método AddStorageProvider()
- [ ] Crear método AddCacheProvider()
- [ ] Crear método AddPasswordHasher()
- [ ] Crear método AddBackgroundJobProcessor()

**Archivo a crear:**
```
✓ Infrastructure/Extensions/ServiceCollectionExtensions.cs
```

#### Día 17: Program.cs Configuration
- [ ] Registrar DbContext
- [ ] Registrar repositorios
- [ ] Registrar providers
- [ ] Configurar Hangfire
- [ ] Configurar Redis

**Modificar:**
```
✓ Program.cs
```

#### Día 18: Testing Setup
- [ ] Crear project AspNetProject.Tests
- [ ] Configurar xUnit
- [ ] Configurar Moq
- [ ] Crear test fixtures

**Crear:**
```
✓ AspNetProject.Tests.csproj
✓ UnitTests/Repositories/DocumentRepositoryTests.cs (ejemplo)
```

#### Día 19-20: Validación y Documentación
- [ ] Validar compilación
- [ ] Crear migraciones adicionales si es necesario
- [ ] Documentar configuración
- [ ] Crear guías de setup

---

## 🎯 HITOS CLAVE

### Milestone 1: Base de Datos Funcional
**Cuándo:** Fin de Día 5  
**Qué:** ApplicationDbContext + Migrations + Schema SQL  
**Validación:** `dotnet ef database update` sin errores

### Milestone 2: Todos los Repositorios
**Cuándo:** Fin de Día 10  
**Qué:** 7 repositorios implementados  
**Validación:** Unit tests pasan

### Milestone 3: Storage y Providers
**Cuándo:** Fin de Día 15  
**Qué:** FileSystem, Azure, BCrypt, Redis, Hangfire  
**Validación:** Compilación exitosa

### Milestone 4: Todo Integrado
**Cuándo:** Fin de Día 20  
**Qué:** Inyección de dependencias en Program.cs  
**Validación:** Aplicación inicia sin errores

---

## 📦 DEPENDENCIAS A INSTALAR

```bash
# EF Core
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools

# Caché
dotnet add package StackExchange.Redis

# Password Hashing
dotnet add package BCrypt.Net-Next

# Background Jobs
dotnet add package Hangfire
dotnet add package Hangfire.SqlServer

# (Opcional) Azure Storage
dotnet add package Azure.Storage.Blobs

# Testing
dotnet add package xunit
dotnet add package Moq
dotnet add package FluentAssertions
```

---

## 💡 CONSIDERACIONES IMPORTANTES

### Multi-Tenancy
✅ Cada query debe incluir tenantId  
✅ Usar shadow properties en EF si es necesario  
✅ Validar en cada operación

### Índices
✅ TenantId + Clave primaria  
✅ Email (por tenant)  
✅ Status (para filtrados)  
✅ OccurredAt (para auditoría)  
✅ DocumentId (para versions)

### Transacciones
✅ Usar DbContext.SaveChangesAsync dentro de transacción  
✅ Para auditoría: crear dentro de misma transacción  
✅ Rollback automático en excepción

### Performance
✅ No cargar todas las relaciones (lazy loading)  
✅ AsNoTracking() para queries de lectura  
✅ Incluir (Include) solo lo necesario  
✅ Usar Select() para proyecciones

### Seguridad
✅ Nunca concatenar SQL  
✅ Usar parámetros siempre  
✅ EF Core los maneja automáticamente  
✅ Validar tenantId en cada operación

---

## 🧪 TESTING STRATEGY

### Unit Tests
- Repositorios con DbContext en-memory
- Mocking de Storage, Cache, Jobs
- Tests para validaciones de multi-tenancy

### Integration Tests
- DbContext real (SQL Server local)
- Storage real (File System)
- Cache real (Redis en Docker)

### Database Tests
- Migraciones aplicadas correctamente
- Índices creados
- Schema válido

---

## 📝 DOCUMENTACIÓN A CREAR

1. `Infrastructure/README.md` - Guía general
2. `Infrastructure/Persistence/README.md` - Guía de persistencia
3. `Infrastructure/Storage/README.md` - Guía de almacenamiento
4. `Infrastructure/Providers/README.md` - Guía de providers
5. `INFRASTRUCTURE_SETUP.md` - Setup guide
6. `DATABASE_SCHEMA.md` - Esquema de BD
7. `CONFIGURATION_GUIDE.md` - Guía de configuración

---

## ✅ DEFINICIÓN DE DONE

Para cada componente:
- [ ] Código implementado
- [ ] Compilación exitosa
- [ ] Tests unitarios (70%+ cobertura)
- [ ] Documentación XML
- [ ] README actualizado
- [ ] 0 errores, 0 warnings

Para toda la fase:
- [ ] Todos los repositorios implementados
- [ ] Storage funcional (dev + prod)
- [ ] Providers integrados
- [ ] Program.cs configurado
- [ ] Aplicación inicia sin errores
- [ ] Tests pasan

---

## 🚀 SIGUIENTES PASOS DESPUÉS DE FASE 3

1. **Fase 4:** Implementar Services (2-3 semanas)
   - DocumentService
   - UserService
   - AuditService
   - TenantService
   - DocumentProcessingService

2. **Fase 5:** Actualizar Controllers (1-2 semanas)
   - Cambiar llamadas a sync → async
   - Crear nuevos endpoints
   - DTOs para requests/responses

3. **Fase 6:** Testing Completo (2-3 semanas)
   - Unit tests completos
   - Integration tests
   - E2E tests
   - Performance tests

---

**Generado por:** GitHub Copilot  
**Status:** Plan Listo  
**Próxima Acción:** Comenzar Día 1 - Setup EF Core

