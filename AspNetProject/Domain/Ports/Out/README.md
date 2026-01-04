# 🔌 Puertos de Salida (Outbound Ports)

## Descripción

Los puertos de salida definen los contratos para acceder a recursos externos:
- **Repositorios:** Abstracción para acceso a base de datos
- **Adaptadores:** Abstracción para servicios externos (almacenamiento, caché, etc.)
- **Proveedores:** Abstracción para integraciones externas

## Estructura

```
Ports/Out/
├── Repositories/           # Puertos de persistencia
│   ├── IDocumentRepository.cs
│   ├── IDocumentVersionRepository.cs
│   ├── IDocumentProcessingJobRepository.cs
│   ├── IUserRepository.cs
│   ├── IAuditLogRepository.cs
│   └── ITenantRepository.cs
│
├── Storage/                # Puertos de almacenamiento
│   ├── IDocumentStorage.cs
│   └── IStorageProvider.cs
│
├── Providers/              # Puertos de integraciones externas
│   ├── IPasswordHasher.cs
│   ├── IEmailProvider.cs (opcional)
│   └── ITelemetryProvider.cs (opcional)
│
└── Cache/                  # Puertos de caché
    └── ICacheProvider.cs
```

## Principios

### 1. **Escalabilidad**
- Métodos async con CancellationToken
- Paginación en queries
- Índices y consultas optimizadas

### 2. **Seguridad**
- Multi-tenancy explícito en queries
- Validación de parámetros
- No exponer datos sensibles

### 3. **Rendimiento**
- Lazy loading considerado
- Caching de datos frecuentes
- Índices en campos clave

### 4. **Mantenibilidad**
- Interfaces claras y específicas
- Responsabilidad única
- Fácil de mockear para tests

## Convenciones de Nombres

- **Repositories:** `IXxxRepository` - CRUD y queries específicas del dominio
- **Storage:** `IXxxStorage` - Almacenamiento de archivos/blobs
- **Providers:** `IXxxProvider` - Servicios externos específicos
- **Cache:** `ICacheProvider` - Gestión de caché genérica

## Relación con In Ports

```
In Port (IDocumentService)
        ↓
    Service Implementation
        ↓
Out Port (IDocumentRepository)
        ↓
    EF Core DbContext / External Service
```

## Próximos Pasos

1. Crear puertos de repositorios (Persistencia)
2. Crear puertos de almacenamiento (Storage)
3. Crear puertos de proveedores (Utilities)
4. Implementar en Infrastructure layer

