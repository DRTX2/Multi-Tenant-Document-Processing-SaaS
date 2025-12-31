# 🔌 Puertos (Ports) - Interfaces del Dominio

Los **puertos** son las interfaces que definen los contratos entre el núcleo de la aplicación y el mundo exterior.

## Estructura

```
Domain/Ports/
├── In/              (Puertos de Entrada - Inbound Ports)
│   └── IWeatherForecastService.cs
└── Out/             (Puertos de Salida - Outbound Ports)
    ├── IRepository.cs
    └── IWeatherForecastProvider.cs
```

---

## 🔵 Puertos de Entrada (In/)

### ¿Qué son?

Interfaces que definen **casos de uso** o **servicios de aplicación**.

### ¿Quién los implementa?

La capa de **Application** (`Application/Services/`)

### ¿Quién los usa?

Los **adaptadores de entrada** (`Adapters/In/`)

### Características:

- ✅ Definen la lógica de negocio que la aplicación expone
- ✅ Son implementados por servicios de aplicación
- ✅ Son usados por REST Controllers, GraphQL Resolvers, etc.
- ✅ Representan **lo que la aplicación PUEDE HACER**

### Ejemplo:

```csharp
// Domain/Ports/In/IWeatherForecastService.cs
namespace AspNetProject.Domain.Ports.In;

/// <summary>
/// Puerto de ENTRADA - Define qué puede hacer la aplicación
/// </summary>
public interface IWeatherForecastService
{
    IEnumerable<WeatherForecast> GetForecasts(int days);
}
```

**Implementado por:**
```csharp
// Application/Services/WeatherForecastService.cs
public class WeatherForecastService : IWeatherForecastService
{
    // Implementación del caso de uso
}
```

**Usado por:**
```csharp
// Adapters/In/Rest/Controllers/WeatherForecastController.cs
public class WeatherForecastController : ControllerBase
{
    private readonly IWeatherForecastService _service; // ← Puerto IN
}
```

---

## 🟢 Puertos de Salida (Out/)

### ¿Qué son?

Interfaces que definen **dependencias externas** que la aplicación necesita.

### ¿Quién los implementa?

La capa de **Infrastructure** (`Infrastructure/Repositories/`, `Infrastructure/Providers/`)

### ¿Quién los usa?

La capa de **Application** (`Application/Services/`)

### Características:

- ✅ Definen las dependencias que la aplicación necesita
- ✅ Son implementados por adaptadores de infraestructura
- ✅ Son usados por servicios de aplicación
- ✅ Representan **lo que la aplicación NECESITA**

### Ejemplo:

```csharp
// Domain/Ports/Out/IWeatherForecastProvider.cs
namespace AspNetProject.Domain.Ports.Out;

/// <summary>
/// Puerto de SALIDA - Define qué necesita la aplicación
/// </summary>
public interface IWeatherForecastProvider
{
    IEnumerable<WeatherForecast> GetForecasts(int days);
}
```

**Implementado por:**
```csharp
// Infrastructure/Providers/RandomWeatherForecastProvider.cs
public class RandomWeatherForecastProvider : IWeatherForecastProvider
{
    // Implementación específica de infraestructura
}
```

**Usado por:**
```csharp
// Application/Services/WeatherForecastService.cs
public class WeatherForecastService : IWeatherForecastService
{
    private readonly IWeatherForecastProvider _provider; // ← Puerto OUT
}
```

---

## 📊 Flujo Completo

```
┌──────────────────────────────────────────────────────────────┐
│                    FLUJO DE DEPENDENCIAS                     │
└──────────────────────────────────────────────────────────────┘

1. Adaptador de Entrada (Adapters/In/)
   │
   │  WeatherForecastController
   │  depende de ↓
   │
   ▼
2. Puerto de Entrada (Domain/Ports/In/)
   │
   │  IWeatherForecastService
   │  implementado por ↓
   │
   ▼
3. Servicio de Aplicación (Application/Services/)
   │
   │  WeatherForecastService
   │  depende de ↓
   │
   ▼
4. Puerto de Salida (Domain/Ports/Out/)
   │
   │  IWeatherForecastProvider
   │  implementado por ↓
   │
   ▼
5. Adaptador de Salida (Infrastructure/)
   │
   │  RandomWeatherForecastProvider
   │
   └─────────────────────────────────────────────────────────────
```

---

## 🎯 Reglas de Oro

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
| `Domain/Ports/` | Cualquier otra capa | El dominio debe ser independiente |
| `Adapters/In/` | `Adapters/Out/` | Los adaptadores no se conocen entre sí |
| `Adapters/In/` | `Infrastructure/` | Debe pasar por Application |
| `Application/` | `Adapters/` | Application no conoce los adaptadores |

---

## 💡 Preguntas Frecuentes

### ¿Por qué separar In/ y Out/?

**Respuesta:** Para que quede **cristalino** qué interfaces son para entrada (casos de uso) y cuáles para salida (dependencias).

### ¿Cuándo crear un puerto In?

**Respuesta:** Cuando quieres exponer un **caso de uso** o **funcionalidad** de tu aplicación.

**Ejemplo:** `IUserService`, `IOrderService`, `IPaymentService`

### ¿Cuándo crear un puerto Out?

**Respuesta:** Cuando tu aplicación **necesita** algo externo.

**Ejemplo:** `IRepository<T>`, `IEmailSender`, `IPaymentGateway`, `IFileStorage`

### ¿Puede un servicio usar múltiples puertos Out?

**Sí, absolutamente:**

```csharp
public class OrderService : IOrderService  // Puerto IN
{
    private readonly IRepository<Order, int> _orderRepo;      // Puerto OUT
    private readonly IEmailSender _emailSender;               // Puerto OUT
    private readonly IPaymentGateway _paymentGateway;         // Puerto OUT
    
    public async Task CreateOrderAsync(Order order)
    {
        await _orderRepo.AddAsync(order);
        await _emailSender.SendOrderConfirmationAsync(order);
        await _paymentGateway.ProcessPaymentAsync(order);
    }
}
```

---

## 📚 Comparación de Nomenclaturas

| Este Proyecto | Alternativas Comunes |
|---------------|---------------------|
| `Domain/Ports/In/` | `Domain/Ports/Input/`, `Application/Ports/`, `UseCases/` |
| `Domain/Ports/Out/` | `Domain/Ports/Output/`, `Domain/Ports/Spi/`, `Infrastructure/Ports/` |

**Elegimos In/Out porque:**
- ✅ Es simple y directo
- ✅ Evita confusión con Primary/Secondary
- ✅ Es consistente con Inbound/Outbound
- ✅ Fácil de entender para nuevos desarrolladores

---

## 🎓 Ejemplo Completo

### Caso de Uso: Enviar Email de Bienvenida

#### 1. Definir Puerto IN (lo que la app PUEDE HACER)

```csharp
// Domain/Ports/In/IUserService.cs
public interface IUserService
{
    Task RegisterUserAsync(string email, string password);
}
```

#### 2. Definir Puertos OUT (lo que la app NECESITA)

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

#### 3. Implementar Servicio (Application)

```csharp
// Application/Services/UserService.cs
public class UserService : IUserService
{
    private readonly IUserRepository _userRepo;    // Puerto OUT
    private readonly IEmailSender _emailSender;    // Puerto OUT
    
    public async Task RegisterUserAsync(string email, string password)
    {
        var user = new User { Email = email, Password = HashPassword(password) };
        await _userRepo.CreateAsync(user);
        await _emailSender.SendWelcomeEmailAsync(email);
    }
}
```

#### 4. Implementar Adaptadores OUT (Infrastructure)

```csharp
// Infrastructure/Repositories/UserRepository.cs
public class UserRepository : IUserRepository
{
    // Implementación con EF Core
}

// Infrastructure/EmailSenders/SmtpEmailSender.cs
public class SmtpEmailSender : IEmailSender
{
    // Implementación con SMTP
}
```

#### 5. Crear Adaptador IN (Adapters/In)

```csharp
// Adapters/In/Rest/Controllers/UserController.cs
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;  // Puerto IN
    
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        await _userService.RegisterUserAsync(request.Email, request.Password);
        return Ok();
    }
}
```

---

## ✅ Checklist para Crear Puertos

### Puerto IN (Caso de Uso)

- [ ] Crear interface en `Domain/Ports/In/`
- [ ] Definir métodos que representen casos de uso
- [ ] Implementar en `Application/Services/`
- [ ] Usar desde `Adapters/In/`
- [ ] Documentar con XML comments

### Puerto OUT (Dependencia)

- [ ] Crear interface en `Domain/Ports/Out/`
- [ ] Definir métodos que la aplicación necesita
- [ ] Implementar en `Infrastructure/`
- [ ] Usar desde `Application/Services/`
- [ ] Registrar en `Program.cs` (DI)

---

**La separación In/Out hace que la arquitectura sea más clara y fácil de entender.** 🎯
