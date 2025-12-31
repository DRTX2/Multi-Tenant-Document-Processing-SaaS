# 📚 Índice de Documentación - AspNetProject

## 🎯 Guías de Inicio Rápido

| Documento | Descripción | Cuándo Leerlo |
|-----------|-------------|---------------|
| [QUICK_START.md](QUICK_START.md) | ⚡ Inicio en 3 pasos | **PRIMERO** - Para empezar rápido |
| [README.md](README.md) | 📖 Guía principal del proyecto | Después del Quick Start |
| [RESUMEN_FINAL.md](RESUMEN_FINAL.md) | 📋 Resumen ejecutivo completo | Para entender qué se implementó |

---

## 🏗️ Arquitectura Hexagonal

### Conceptos Fundamentales

| Documento | Descripción | Nivel |
|-----------|-------------|-------|
| [PROJECT_STRUCTURE.md](PROJECT_STRUCTURE.md) | 📁 **Estructura completa con In/Out** | **ESENCIAL** ⭐ |
| [Domain/Ports/README.md](AspNetProject/Domain/Ports/README.md) | 🔌 **Puertos In/Out explicados** | **ESENCIAL** ⭐ |
| [Adapters/README.md](AspNetProject/Adapters/README.md) | 🔌 Adaptadores In/Out explicados | Básico |
| [ARCHITECTURE.md](ARCHITECTURE.md) | 🏗️ Diagramas y principios SOLID | Intermedio |

### Refactorización Reciente

| Documento | Descripción | Cuándo Leerlo |
|-----------|-------------|---------------|
| [REFACTORING_SUMMARY.md](REFACTORING_SUMMARY.md) | ✅ **Resumen de cambios a In/Out** | **Para entender los cambios** ⭐ |
| [BEFORE_AFTER_COMPARISON.md](BEFORE_AFTER_COMPARISON.md) | 🔄 Comparación visual antes/después | Para ver las mejoras |

---

## 🔧 Guías de Extensión

| Documento | Descripción | Cuándo Usarlo |
|-----------|-------------|---------------|
| [EXTENSION_GUIDE.md](EXTENSION_GUIDE.md) | 🔧 Cómo extender el proyecto | Al agregar funcionalidad |
| [ADDING_ADAPTERS_GUIDE.md](ADDING_ADAPTERS_GUIDE.md) | 🚀 Agregar GraphQL, gRPC, CLI, etc. | Al agregar nuevos adaptadores |

---

## 🗄️ Base de Datos y Entity Framework

| Documento | Descripción | Nivel |
|-----------|-------------|-------|
| [DATABASE_GUIDE.md](DATABASE_GUIDE.md) | 🗄️ Guía completa de EF Core | Intermedio |
| [EF_CORE_SUMMARY.md](EF_CORE_SUMMARY.md) | 📊 Resumen de EF Core | Básico |

---

## 📖 Documentación por Tema

### Para Principiantes

1. **Empezar** → [QUICK_START.md](QUICK_START.md)
2. **Entender la estructura** → [PROJECT_STRUCTURE.md](PROJECT_STRUCTURE.md) ⭐
3. **Entender In/Out** → [Domain/Ports/README.md](AspNetProject/Domain/Ports/README.md) ⭐
4. **Ver la arquitectura** → [ARCHITECTURE.md](ARCHITECTURE.md)

### Para Desarrolladores

1. **Agregar una entidad** → [QUICK_START.md](QUICK_START.md#-agregar-una-nueva-entidad-5-minutos)
2. **Extender funcionalidad** → [EXTENSION_GUIDE.md](EXTENSION_GUIDE.md)
3. **Agregar adaptadores** → [ADDING_ADAPTERS_GUIDE.md](ADDING_ADAPTERS_GUIDE.md)
4. **Trabajar con EF Core** → [DATABASE_GUIDE.md](DATABASE_GUIDE.md)

### Para Arquitectos

1. **Entender la refactorización** → [REFACTORING_SUMMARY.md](REFACTORING_SUMMARY.md) ⭐
2. **Comparar antes/después** → [BEFORE_AFTER_COMPARISON.md](BEFORE_AFTER_COMPARISON.md)
3. **Principios SOLID** → [ARCHITECTURE.md](ARCHITECTURE.md#principios-solid-aplicados)
4. **Estructura completa** → [PROJECT_STRUCTURE.md](PROJECT_STRUCTURE.md)

---

## 🎓 Conceptos Clave Explicados

### ¿Qué es la Arquitectura Hexagonal?

Una arquitectura que separa el **núcleo de negocio** (Domain + Application) de los **detalles técnicos** (Adapters + Infrastructure).

**Leer:** [ARCHITECTURE.md](ARCHITECTURE.md)

---

### ¿Qué son In y Out? ⭐⭐⭐

**In (Entrada/Inbound):** Todo lo que **ENTRA** a tu aplicación
- **Adaptadores In:** REST, GraphQL, CLI → Reciben peticiones
- **Puertos In:** Interfaces de casos de uso → Definen qué puede hacer la app

**Out (Salida/Outbound):** Todo lo que **SALE** de tu aplicación
- **Adaptadores Out:** Repositories, APIs → Implementan dependencias
- **Puertos Out:** Interfaces de dependencias → Definen qué necesita la app

**Leer:** [Domain/Ports/README.md](AspNetProject/Domain/Ports/README.md) ← **¡Empieza aquí!** ⭐

---

### ¿Por qué In/Out en lugar de Primary/Secondary?

**In/Out es más claro:**
- ✅ **In** = **IN**gresa a la aplicación (entrada)
- ✅ **Out** = **OUT**sourcing de dependencias (salida)
- ✅ Auto-explicativo, no requiere memorizar términos

**Primary/Secondary es confuso:**
- ❓ ¿Qué significa "Primary"?
- ❓ ¿Por qué "Secondary"?
- ❓ Requiere explicación adicional

**Leer:** [REFACTORING_SUMMARY.md](REFACTORING_SUMMARY.md#-por-qué-inout-en-lugar-de-primarysecondary)

---

### ¿Cómo funciona el Repository Pattern?

Usamos un `IRepository<TEntity, TId>` genérico que funciona con cualquier entidad.

**Leer:** [DATABASE_GUIDE.md](DATABASE_GUIDE.md#-repository-pattern-genérico)

---

## 🗂️ Estructura del Proyecto

```
AspNetProject/
├── Adapters/                    🔌 Adaptadores
│   ├── In/                      🔵 Entrada (REST, GraphQL, gRPC)
│   └── Out/                     🟢 Salida (opcional)
│
├── Domain/                      💎 Núcleo (Core)
│   ├── Models/                  Entidades
│   └── Ports/                   Interfaces (Puertos)
│       ├── In/                  🔵 Puertos de Entrada
│       └── Out/                 🟢 Puertos de Salida
│
├── Application/                 📋 Casos de Uso
│   └── Services/                Lógica de negocio
│
└── Infrastructure/              🔧 Infraestructura
    ├── Data/                    DbContext, Configurations
    ├── Providers/               Servicios externos
    └── Repositories/            Acceso a datos
```

**Leer:** [PROJECT_STRUCTURE.md](PROJECT_STRUCTURE.md)

---

## 🚀 Casos de Uso Comunes

### Quiero agregar una nueva entidad

1. Crear entidad en `Domain/Models/`
2. Crear configuración en `Infrastructure/Data/Configurations/`
3. Agregar DbSet en `ApplicationDbContext`
4. Crear migración
5. Usar `IRepository<TEntity, TId>` en servicios

**Guía:** [QUICK_START.md](QUICK_START.md#-agregar-una-nueva-entidad-5-minutos)

---

### Quiero agregar GraphQL

1. Instalar `HotChocolate.AspNetCore`
2. Crear Query en `Adapters/In/GraphQL/Queries/`
3. Registrar en `Program.cs`
4. Probar en `/graphql`

**Guía:** [ADDING_ADAPTERS_GUIDE.md](ADDING_ADAPTERS_GUIDE.md#1-agregar-graphql-adaptador-primario)

---

### Quiero agregar gRPC

1. Instalar `Grpc.AspNetCore`
2. Crear `.proto` en `Adapters/In/Grpc/Protos/`
3. Crear servicio en `Adapters/In/Grpc/Services/`
4. Registrar en `Program.cs`

**Guía:** [ADDING_ADAPTERS_GUIDE.md](ADDING_ADAPTERS_GUIDE.md#2-agregar-grpc-adaptador-primario)

---

### Quiero enviar emails

1. Definir `IEmailSender` en `Domain/Ports/Out/`
2. Implementar en `Infrastructure/EmailSenders/`
3. Registrar en `Program.cs`
4. Usar en Application layer

**Guía:** [ADDING_ADAPTERS_GUIDE.md](ADDING_ADAPTERS_GUIDE.md#4-agregar-email-sender-adaptador-secundario)

---

## 📊 Diagramas

### Flujo de una Petición

```
Cliente HTTP  →  REST Controller  →  Service  →  Provider  →  Respuesta
   (In)           (Adapters/In)    (Application) (Infrastructure)
```

**Ver diagrama completo:** [ARCHITECTURE.md](ARCHITECTURE.md#flujo-de-una-petición)

---

### Capas de la Arquitectura

```
┌─────────────────────────────────────┐
│  Adapters/In (Inbound)              │  🔵 Entrada
├─────────────────────────────────────┤
│  Application (Use Cases)            │  📋 Lógica de negocio
├─────────────────────────────────────┤
│  Domain (Core)                      │  💎 Núcleo puro
│  ├── Ports/In/                      │  🔵 Casos de uso
│  └── Ports/Out/                     │  🟢 Dependencias
├─────────────────────────────────────┤
│  Infrastructure (Outbound)          │  🟢 Salida
└─────────────────────────────────────┘
```

**Ver diagrama completo:** [PROJECT_STRUCTURE.md](PROJECT_STRUCTURE.md#flujo-de-dependencias)

---

## 🔗 Enlaces Rápidos

| Necesito... | Ir a... |
|-------------|---------|
| Empezar rápido | [QUICK_START.md](QUICK_START.md) |
| Entender In/Out | [Domain/Ports/README.md](AspNetProject/Domain/Ports/README.md) ⭐⭐⭐ |
| Ver la estructura | [PROJECT_STRUCTURE.md](PROJECT_STRUCTURE.md) ⭐ |
| Agregar funcionalidad | [EXTENSION_GUIDE.md](EXTENSION_GUIDE.md) |
| Agregar adaptadores | [ADDING_ADAPTERS_GUIDE.md](ADDING_ADAPTERS_GUIDE.md) |
| Trabajar con BD | [DATABASE_GUIDE.md](DATABASE_GUIDE.md) |
| Ver cambios recientes | [REFACTORING_SUMMARY.md](REFACTORING_SUMMARY.md) ⭐ |

---

## 🎯 Ruta de Aprendizaje Recomendada

### Nivel 1: Básico (30 minutos)

1. ✅ [QUICK_START.md](QUICK_START.md) - Ejecutar el proyecto
2. ✅ [PROJECT_STRUCTURE.md](PROJECT_STRUCTURE.md) - Entender la estructura ⭐
3. ✅ [Domain/Ports/README.md](AspNetProject/Domain/Ports/README.md) - **Concepto In/Out** ⭐⭐⭐

### Nivel 2: Intermedio (1 hora)

4. ✅ [Adapters/README.md](AspNetProject/Adapters/README.md) - Adaptadores In/Out
5. ✅ [ARCHITECTURE.md](ARCHITECTURE.md) - Principios SOLID
6. ✅ [DATABASE_GUIDE.md](DATABASE_GUIDE.md) - EF Core
7. ✅ [EXTENSION_GUIDE.md](EXTENSION_GUIDE.md) - Extender el proyecto

### Nivel 3: Avanzado (2 horas)

8. ✅ [ADDING_ADAPTERS_GUIDE.md](ADDING_ADAPTERS_GUIDE.md) - GraphQL, gRPC, CLI
9. ✅ [REFACTORING_SUMMARY.md](REFACTORING_SUMMARY.md) - Decisiones arquitectónicas ⭐
10. ✅ [BEFORE_AFTER_COMPARISON.md](BEFORE_AFTER_COMPARISON.md) - Evolución del proyecto

---

## ❓ FAQ - Preguntas Frecuentes

### ¿Qué significa "In"?

**Respuesta:** **In** = **IN**bound = **Entrada**. Todo lo que entra a tu aplicación (REST, GraphQL, CLI).

**Leer:** [Domain/Ports/README.md](AspNetProject/Domain/Ports/README.md#-puertos-de-entrada-in)

---

### ¿Qué significa "Out"?

**Respuesta:** **Out** = **OUT**bound = **Salida**. Todo lo que sale de tu aplicación (Database, Email, APIs).

**Leer:** [Domain/Ports/README.md](AspNetProject/Domain/Ports/README.md#-puertos-de-salida-out)

---

### ¿Dónde van los puertos de entrada?

**Respuesta:** En `Domain/Ports/In/`. Son interfaces de casos de uso implementadas por `Application/Services/`.

**Leer:** [Domain/Ports/README.md](AspNetProject/Domain/Ports/README.md#-puertos-de-entrada-in)

---

### ¿Dónde van los puertos de salida?

**Respuesta:** En `Domain/Ports/Out/`. Son interfaces de dependencias implementadas por `Infrastructure/`.

**Leer:** [Domain/Ports/README.md](AspNetProject/Domain/Ports/README.md#-puertos-de-salida-out)

---

### ¿Cómo agrego una nueva entidad?

**Respuesta:** 5 pasos: Entidad → Configuración → DbSet → Migración → Usar.

**Leer:** [QUICK_START.md](QUICK_START.md#-agregar-una-nueva-entidad-5-minutos)

---

### ¿Cómo agrego GraphQL?

**Respuesta:** Instalar HotChocolate → Crear Query en `Adapters/In/GraphQL/` → Registrar → Probar.

**Leer:** [ADDING_ADAPTERS_GUIDE.md](ADDING_ADAPTERS_GUIDE.md#1-agregar-graphql-adaptador-primario)

---

## 📞 Soporte

Si tienes dudas:

1. **Busca en este índice** el tema relacionado
2. **Lee el documento recomendado**
3. **Revisa los ejemplos de código** en cada guía
4. **Consulta los diagramas** en ARCHITECTURE.md

---

## 🎓 Recursos Externos

- [Hexagonal Architecture (Alistair Cockburn)](https://alistair.cockburn.us/hexagonal-architecture/)
- [Clean Architecture (Robert C. Martin)](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Ports and Adapters Pattern](https://herbertograca.com/2017/09/14/ports-adapters-architecture/)
- [Entity Framework Core Docs](https://learn.microsoft.com/en-us/ef/core/)

---

## 📝 Notas de la Última Actualización

**Fecha:** 2025-12-31

**Cambios principales:**
- ✅ Refactorización completa a nomenclatura **In/Out**
- ✅ Separación de puertos en `Domain/Ports/In/` y `Domain/Ports/Out/`
- ✅ Adaptadores en `Adapters/In/` y `Adapters/Out/`
- ✅ Documentación completa de In/Out
- ✅ Guías actualizadas con nueva estructura

**Ver detalles:** [REFACTORING_SUMMARY.md](REFACTORING_SUMMARY.md)

---

**¡Bienvenido a AspNetProject!** 🚀

Comienza por [QUICK_START.md](QUICK_START.md) y luego lee [Domain/Ports/README.md](AspNetProject/Domain/Ports/README.md) para entender el concepto clave de **In/Out**.
