# 🎯 In/Out - Guía Visual Rápida

## 🔵 IN (Entrada/Inbound)

### Todo lo que **ENTRA** a tu aplicación

```
Cliente Externo  ──────►  Tu Aplicación
                (entra)
```

### Pregunta clave:
**"¿Qué puede hacer mi aplicación?"**

---

### 📍 Ubicaciones

| Tipo | Ubicación | Qué es |
|------|-----------|--------|
| **Adaptadores In** | `Adapters/In/` | REST, GraphQL, gRPC, CLI |
| **Puertos In** | `Domain/Ports/In/` | Interfaces de casos de uso |

---

### 💡 Ejemplos

#### Adaptador In (REST Controller)

```csharp
// Adapters/In/Rest/Controllers/WeatherForecastController.cs
[ApiController]
public class WeatherForecastController : ControllerBase
{
    private readonly IWeatherForecastService _service; // ← Puerto IN
    
    [HttpGet]
    public IActionResult Get(int days)
    {
        // Cliente HTTP ENTRA con petición
        var forecasts = _service.GetForecasts(days);
        return Ok(forecasts);
    }
}
```

#### Puerto In (Caso de Uso)

```csharp
// Domain/Ports/In/IWeatherForecastService.cs
public interface IWeatherForecastService
{
    // Define QUÉ puede hacer la aplicación
    IEnumerable<WeatherForecast> GetForecasts(int days);
}
```

#### Implementación (Application)

```csharp
// Application/Services/WeatherForecastService.cs
public class WeatherForecastService : IWeatherForecastService // Implementa puerto IN
{
    public IEnumerable<WeatherForecast> GetForecasts(int days)
    {
        // Lógica de negocio
    }
}
```

---

## 🟢 OUT (Salida/Outbound)

### Todo lo que **SALE** de tu aplicación

```
Tu Aplicación  ──────►  Recursos Externos
               (sale)
```

### Pregunta clave:
**"¿Qué necesita mi aplicación?"**

---

### 📍 Ubicaciones

| Tipo | Ubicación | Qué es |
|------|-----------|--------|
| **Adaptadores Out** | `Infrastructure/` | Repositories, Providers, APIs |
| **Puertos Out** | `Domain/Ports/Out/` | Interfaces de dependencias |

---

### 💡 Ejemplos

#### Puerto Out (Dependencia)

```csharp
// Domain/Ports/Out/IWeatherForecastProvider.cs
public interface IWeatherForecastProvider
{
    // Define QUÉ necesita la aplicación
    IEnumerable<WeatherForecast> GetForecasts(int days);
}
```

#### Adaptador Out (Provider)

```csharp
// Infrastructure/Providers/RandomWeatherForecastProvider.cs
public class RandomWeatherForecastProvider : IWeatherForecastProvider // Implementa puerto OUT
{
    public IEnumerable<WeatherForecast> GetForecasts(int days)
    {
        // Tu aplicación SALE para obtener datos
        return GenerateRandomData(days);
    }
}
```

#### Uso en Application

```csharp
// Application/Services/WeatherForecastService.cs
public class WeatherForecastService : IWeatherForecastService
{
    private readonly IWeatherForecastProvider _provider; // ← Puerto OUT
    
    public IEnumerable<WeatherForecast> GetForecasts(int days)
    {
        // Usa puerto OUT para obtener datos
        return _provider.GetForecasts(days);
    }
}
```

---

## 📊 Flujo Completo

```
┌─────────────────────────────────────────────────────────────┐
│                    FLUJO DE DATOS                           │
└─────────────────────────────────────────────────────────────┘

🔵 IN (Entrada)
│
│  1. Cliente HTTP hace petición
│     ↓
│  2. WeatherForecastController (Adapters/In/)
│     ↓
│  3. IWeatherForecastService (Domain/Ports/In/)
│     ↓
│  4. WeatherForecastService (Application/)
│
├─────────────────────────────────────────────────────────────┤
│
🟢 OUT (Salida)
│
│  5. IWeatherForecastProvider (Domain/Ports/Out/)
│     ↓
│  6. RandomWeatherForecastProvider (Infrastructure/)
│     ↓
│  7. Genera/Obtiene datos
│
└─────────────────────────────────────────────────────────────┘
```

---

## 🎯 Reglas Simples

### ✅ Permitido

| Desde | Hacia | Ejemplo |
|-------|-------|---------|
| `Adapters/In/` | `Domain/Ports/In/` | Controller → IWeatherForecastService |
| `Application/` | `Domain/Ports/In/` | Service implementa IWeatherForecastService |
| `Application/` | `Domain/Ports/Out/` | Service → IWeatherForecastProvider |
| `Infrastructure/` | `Domain/Ports/Out/` | Provider implementa IWeatherForecastProvider |

### ❌ Prohibido

| Desde | Hacia | Por qué |
|-------|-------|---------|
| `Domain/Ports/` | Cualquier capa | Domain debe ser independiente |
| `Adapters/In/` | `Infrastructure/` | Debe pasar por Application |
| `Application/` | `Adapters/` | Application no conoce adaptadores |

---

## 💡 Reglas Nemotécnicas

### Para recordar IN

```
IN  = INgresa
      ↓
   Lo que ENTRA
      ↓
   REST, GraphQL, CLI
```

### Para recordar OUT

```
OUT = OUTsourcing
      ↓
   Lo que SALE
      ↓
   Database, Email, APIs
```

---

## 📁 Estructura Visual

```
AspNetProject/
│
├── 🔵 ENTRADA (IN)
│   │
│   ├── Adapters/In/              ← Reciben peticiones
│   │   ├── Rest/
│   │   ├── GraphQL/
│   │   └── Grpc/
│   │
│   └── Domain/Ports/In/          ← Definen casos de uso
│       └── IWeatherForecastService.cs
│
├── 💎 CORE
│   │
│   ├── Domain/Models/            ← Entidades
│   │
│   └── Application/Services/     ← Lógica de negocio
│       └── WeatherForecastService.cs
│
└── 🟢 SALIDA (OUT)
    │
    ├── Domain/Ports/Out/         ← Definen dependencias
    │   ├── IRepository.cs
    │   └── IWeatherForecastProvider.cs
    │
    └── Infrastructure/           ← Implementan dependencias
        ├── Repositories/
        └── Providers/
```

---

## 🎓 Ejemplo Completo: Registro de Usuario

### 1. Puerto IN (Caso de Uso)

```csharp
// Domain/Ports/In/IUserService.cs
public interface IUserService
{
    Task RegisterAsync(string email, string password);
}
```

### 2. Puertos OUT (Dependencias)

```csharp
// Domain/Ports/Out/IUserRepository.cs
public interface IUserRepository
{
    Task<User> CreateAsync(User user);
}

// Domain/Ports/Out/IEmailSender.cs
public interface IEmailSender
{
    Task SendWelcomeEmailAsync(string email);
}
```

### 3. Servicio (Application)

```csharp
// Application/Services/UserService.cs
public class UserService : IUserService  // Implementa IN
{
    private readonly IUserRepository _repo;      // Usa OUT
    private readonly IEmailSender _emailSender;  // Usa OUT
    
    public async Task RegisterAsync(string email, string password)
    {
        var user = new User { Email = email };
        await _repo.CreateAsync(user);           // OUT
        await _emailSender.SendWelcomeEmailAsync(email); // OUT
    }
}
```

### 4. Adaptador IN (REST)

```csharp
// Adapters/In/Rest/Controllers/UserController.cs
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService _service;  // Usa IN
    
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        await _service.RegisterAsync(req.Email, req.Password);
        return Ok();
    }
}
```

### 5. Adaptadores OUT (Infrastructure)

```csharp
// Infrastructure/Repositories/UserRepository.cs
public class UserRepository : IUserRepository  // Implementa OUT
{
    public async Task<User> CreateAsync(User user)
    {
        // Acceso a base de datos
    }
}

// Infrastructure/EmailSenders/SmtpEmailSender.cs
public class SmtpEmailSender : IEmailSender  // Implementa OUT
{
    public async Task SendWelcomeEmailAsync(string email)
    {
        // Envío de email
    }
}
```

---

## ✅ Checklist Rápido

### Agregar Puerto IN

- [ ] Crear interface en `Domain/Ports/In/`
- [ ] Implementar en `Application/Services/`
- [ ] Usar desde `Adapters/In/`

### Agregar Puerto OUT

- [ ] Crear interface en `Domain/Ports/Out/`
- [ ] Implementar en `Infrastructure/`
- [ ] Usar desde `Application/Services/`
- [ ] Registrar en `Program.cs` (DI)

---

## 🔗 Documentación Completa

- [Domain/Ports/README.md](AspNetProject/Domain/Ports/README.md) - Guía completa de puertos
- [Adapters/README.md](AspNetProject/Adapters/README.md) - Guía completa de adaptadores
- [PROJECT_STRUCTURE.md](PROJECT_STRUCTURE.md) - Estructura completa del proyecto
- [REFACTORING_SUMMARY.md](REFACTORING_SUMMARY.md) - Resumen de cambios

---

**¿Mucho más claro con In/Out, verdad?** 🎯

**IN** = Entrada = Lo que entra  
**OUT** = Salida = Lo que sale  

¡Así de simple! 🚀
