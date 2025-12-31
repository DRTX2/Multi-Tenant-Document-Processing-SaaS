# 🚀 Guía Práctica: Agregar Nuevos Adaptadores

Esta guía muestra cómo aprovechar la nueva estructura `Adapters/` para agregar diferentes tipos de adaptadores a tu aplicación.

---

## 📋 Tabla de Contenidos

1. [Agregar GraphQL (Adaptador Primario)](#1-agregar-graphql-adaptador-primario)
2. [Agregar gRPC (Adaptador Primario)](#2-agregar-grpc-adaptador-primario)
3. [Agregar CLI (Adaptador Primario)](#3-agregar-cli-adaptador-primario)
4. [Agregar Email Sender (Adaptador Secundario)](#4-agregar-email-sender-adaptador-secundario)
5. [Agregar HTTP Client (Adaptador Secundario)](#5-agregar-http-client-adaptador-secundario)

---

## 1. Agregar GraphQL (Adaptador Primario)

### Paso 1: Instalar Paquetes

```bash
dotnet add package HotChocolate.AspNetCore
```

### Paso 2: Crear Query

```csharp
// Adapters/Primary/GraphQL/Queries/WeatherForecastQuery.cs
namespace AspNetProject.Adapters.Primary.GraphQL.Queries;

public class WeatherForecastQuery
{
    private readonly IWeatherForecastService _service;
    
    public WeatherForecastQuery(IWeatherForecastService service)
    {
        _service = service;
    }
    
    /// <summary>
    /// Obtiene pronósticos del tiempo
    /// </summary>
    public IEnumerable<WeatherForecast> GetForecasts(int days = 5)
    {
        return _service.GetForecasts(days);
    }
    
    /// <summary>
    /// Obtiene un pronóstico específico por fecha
    /// </summary>
    public WeatherForecast? GetForecastByDate(DateOnly date)
    {
        var forecasts = _service.GetForecasts(30);
        return forecasts.FirstOrDefault(f => f.Date == date);
    }
}
```

### Paso 3: Crear Mutation (Opcional)

```csharp
// Adapters/Primary/GraphQL/Mutations/WeatherForecastMutation.cs
namespace AspNetProject.Adapters.Primary.GraphQL.Mutations;

public class WeatherForecastMutation
{
    private readonly IWeatherForecastService _service;
    
    public WeatherForecastMutation(IWeatherForecastService service)
    {
        _service = service;
    }
    
    public string RefreshForecasts()
    {
        // Lógica para refrescar pronósticos
        return "Forecasts refreshed successfully";
    }
}
```

### Paso 4: Registrar en Program.cs

```csharp
// Program.cs
builder.Services
    .AddGraphQLServer()
    .AddQueryType<WeatherForecastQuery>()
    .AddMutationType<WeatherForecastMutation>();

// ...

app.MapGraphQL("/graphql");
```

### Paso 5: Probar

```graphql
# Query
query {
  forecasts(days: 7) {
    date
    temperatureC
    summary
  }
}

# Mutation
mutation {
  refreshForecasts
}
```

**Acceder a GraphQL Playground:** `https://localhost:5001/graphql`

---

## 2. Agregar gRPC (Adaptador Primario)

### Paso 1: Instalar Paquetes

```bash
dotnet add package Grpc.AspNetCore
```

### Paso 2: Crear Proto File

```protobuf
// Adapters/Primary/Grpc/Protos/weather.proto
syntax = "proto3";

option csharp_namespace = "AspNetProject.Adapters.Primary.Grpc";

package weather;

service WeatherService {
  rpc GetForecasts (ForecastRequest) returns (ForecastResponse);
}

message ForecastRequest {
  int32 days = 1;
}

message ForecastResponse {
  repeated WeatherForecast forecasts = 1;
}

message WeatherForecast {
  string date = 1;
  int32 temperatureC = 2;
  string summary = 3;
}
```

### Paso 3: Actualizar .csproj

```xml
<ItemGroup>
  <Protobuf Include="Adapters/Primary/Grpc/Protos/weather.proto" GrpcServices="Server" />
</ItemGroup>
```

### Paso 4: Crear Service

```csharp
// Adapters/Primary/Grpc/Services/WeatherForecastGrpcService.cs
namespace AspNetProject.Adapters.Primary.Grpc.Services;

using global::Grpc.Core;

public class WeatherForecastGrpcService : WeatherService.WeatherServiceBase
{
    private readonly IWeatherForecastService _service;
    
    public WeatherForecastGrpcService(IWeatherForecastService service)
    {
        _service = service;
    }
    
    public override Task<ForecastResponse> GetForecasts(
        ForecastRequest request, 
        ServerCallContext context)
    {
        var forecasts = _service.GetForecasts(request.Days);
        
        var response = new ForecastResponse();
        foreach (var forecast in forecasts)
        {
            response.Forecasts.Add(new WeatherForecast
            {
                Date = forecast.Date.ToString("yyyy-MM-dd"),
                TemperatureC = forecast.TemperatureC,
                Summary = forecast.Summary ?? string.Empty
            });
        }
        
        return Task.FromResult(response);
    }
}
```

### Paso 5: Registrar en Program.cs

```csharp
// Program.cs
builder.Services.AddGrpc();

// ...

app.MapGrpcService<WeatherForecastGrpcService>();
```

### Paso 6: Probar con Cliente

```csharp
// Cliente gRPC
var channel = GrpcChannel.ForAddress("https://localhost:5001");
var client = new WeatherService.WeatherServiceClient(channel);

var response = await client.GetForecastsAsync(new ForecastRequest { Days = 5 });
foreach (var forecast in response.Forecasts)
{
    Console.WriteLine($"{forecast.Date}: {forecast.TemperatureC}°C - {forecast.Summary}");
}
```

---

## 3. Agregar CLI (Adaptador Primario)

### Paso 1: Instalar Paquetes

```bash
dotnet add package System.CommandLine
```

### Paso 2: Crear Command

```csharp
// Adapters/Primary/Cli/Commands/ForecastCommand.cs
namespace AspNetProject.Adapters.Primary.Cli.Commands;

using System.CommandLine;

public class ForecastCommand : Command
{
    private readonly IWeatherForecastService _service;
    
    public ForecastCommand(IWeatherForecastService service) 
        : base("forecast", "Get weather forecasts")
    {
        _service = service;
        
        var daysOption = new Option<int>(
            name: "--days",
            description: "Number of days to forecast",
            getDefaultValue: () => 5);
        
        AddOption(daysOption);
        
        this.SetHandler((int days) =>
        {
            var forecasts = _service.GetForecasts(days);
            
            Console.WriteLine($"\nWeather Forecast for the next {days} days:\n");
            foreach (var forecast in forecasts)
            {
                Console.WriteLine($"{forecast.Date:yyyy-MM-dd}: {forecast.TemperatureC}°C - {forecast.Summary}");
            }
        }, daysOption);
    }
}
```

### Paso 3: Crear CLI Host

```csharp
// Adapters/Primary/Cli/CliHost.cs
namespace AspNetProject.Adapters.Primary.Cli;

using System.CommandLine;

public static class CliHost
{
    public static async Task<int> RunAsync(string[] args, IServiceProvider services)
    {
        var rootCommand = new RootCommand("AspNetProject CLI");
        
        var forecastCommand = new ForecastCommand(
            services.GetRequiredService<IWeatherForecastService>());
        
        rootCommand.AddCommand(forecastCommand);
        
        return await rootCommand.InvokeAsync(args);
    }
}
```

### Paso 4: Actualizar Program.cs

```csharp
// Program.cs
var app = builder.Build();

// Si se pasan argumentos CLI, ejecutar en modo CLI
if (args.Length > 0)
{
    return await CliHost.RunAsync(args, app.Services);
}

// Sino, ejecutar como API web
// ... configuración normal de la API
```

### Paso 5: Probar

```bash
dotnet run -- forecast --days 7
```

---

## 4. Agregar Email Sender (Adaptador Secundario)

### Paso 1: Definir Puerto en Domain

```csharp
// Domain/Ports/IEmailSender.cs
namespace AspNetProject.Domain.Ports;

public interface IEmailSender
{
    Task SendEmailAsync(string to, string subject, string body);
    Task SendEmailAsync(string to, string subject, string body, bool isHtml);
}
```

### Paso 2: Crear Implementación en Infrastructure

```csharp
// Infrastructure/EmailSenders/SmtpEmailSender.cs
namespace AspNetProject.Infrastructure.EmailSenders;

using System.Net;
using System.Net.Mail;

public class SmtpEmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;
    
    public SmtpEmailSender(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public async Task SendEmailAsync(string to, string subject, string body)
    {
        await SendEmailAsync(to, subject, body, isHtml: false);
    }
    
    public async Task SendEmailAsync(string to, string subject, string body, bool isHtml)
    {
        var smtpHost = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
        var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
        var smtpUser = _configuration["Email:SmtpUser"] ?? throw new InvalidOperationException("Email:SmtpUser not configured");
        var smtpPass = _configuration["Email:SmtpPass"] ?? throw new InvalidOperationException("Email:SmtpPass not configured");
        
        using var client = new SmtpClient(smtpHost, smtpPort)
        {
            Credentials = new NetworkCredential(smtpUser, smtpPass),
            EnableSsl = true
        };
        
        var message = new MailMessage
        {
            From = new MailAddress(smtpUser),
            Subject = subject,
            Body = body,
            IsBodyHtml = isHtml
        };
        message.To.Add(to);
        
        await client.SendMailAsync(message);
    }
}
```

### Paso 3: Registrar en Program.cs

```csharp
// Program.cs
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
```

### Paso 4: Usar en Application Layer

```csharp
// Application/Services/NotificationService.cs
namespace AspNetProject.Application.Services;

public class NotificationService
{
    private readonly IEmailSender _emailSender;
    
    public NotificationService(IEmailSender emailSender)
    {
        _emailSender = emailSender;
    }
    
    public async Task SendWeatherAlertAsync(string email, WeatherForecast forecast)
    {
        var subject = "Weather Alert!";
        var body = $"Temperature will be {forecast.TemperatureC}°C on {forecast.Date}";
        
        await _emailSender.SendEmailAsync(email, subject, body);
    }
}
```

---

## 5. Agregar HTTP Client (Adaptador Secundario)

### Paso 1: Definir Puerto en Domain

```csharp
// Domain/Ports/IExternalWeatherProvider.cs
namespace AspNetProject.Domain.Ports;

public interface IExternalWeatherProvider
{
    Task<IEnumerable<WeatherForecast>> GetForecastsFromApiAsync(string city);
}
```

### Paso 2: Crear Implementación en Infrastructure

```csharp
// Infrastructure/Providers/OpenWeatherMapProvider.cs
namespace AspNetProject.Infrastructure.Providers;

using System.Net.Http.Json;

public class OpenWeatherMapProvider : IExternalWeatherProvider
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    
    public OpenWeatherMapProvider(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }
    
    public async Task<IEnumerable<WeatherForecast>> GetForecastsFromApiAsync(string city)
    {
        var apiKey = _configuration["OpenWeatherMap:ApiKey"] 
            ?? throw new InvalidOperationException("OpenWeatherMap:ApiKey not configured");
        
        var url = $"https://api.openweathermap.org/data/2.5/forecast?q={city}&appid={apiKey}&units=metric";
        
        var response = await _httpClient.GetFromJsonAsync<OpenWeatherMapResponse>(url);
        
        if (response?.List == null)
            return Enumerable.Empty<WeatherForecast>();
        
        return response.List.Select(item => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.UnixEpoch.AddSeconds(item.Dt)),
            TemperatureC = (int)item.Main.Temp,
            Summary = item.Weather.FirstOrDefault()?.Description ?? "Unknown"
        });
    }
    
    // DTOs para deserialización
    private record OpenWeatherMapResponse(List<ListItem> List);
    private record ListItem(long Dt, Main Main, List<Weather> Weather);
    private record Main(double Temp);
    private record Weather(string Description);
}
```

### Paso 3: Registrar en Program.cs

```csharp
// Program.cs
builder.Services.AddHttpClient<IExternalWeatherProvider, OpenWeatherMapProvider>(client =>
{
    client.BaseAddress = new Uri("https://api.openweathermap.org");
    client.DefaultRequestHeaders.Add("User-Agent", "AspNetProject");
});
```

### Paso 4: Usar en Application Layer

```csharp
// Application/Services/WeatherForecastService.cs
public class WeatherForecastService : IWeatherForecastService
{
    private readonly IWeatherForecastProvider _localProvider;
    private readonly IExternalWeatherProvider _externalProvider;
    
    public WeatherForecastService(
        IWeatherForecastProvider localProvider,
        IExternalWeatherProvider externalProvider)
    {
        _localProvider = localProvider;
        _externalProvider = externalProvider;
    }
    
    public async Task<IEnumerable<WeatherForecast>> GetForecastsAsync(int days, string? city = null)
    {
        if (city != null)
        {
            // Usar proveedor externo
            return await _externalProvider.GetForecastsFromApiAsync(city);
        }
        
        // Usar proveedor local
        return _localProvider.GetForecasts(days);
    }
}
```

---

## 📊 Resumen de Ubicaciones

| Tipo de Adaptador | Ubicación | Ejemplo |
|-------------------|-----------|---------|
| **REST API** | `Adapters/Primary/Rest/Controllers/` | `WeatherForecastController.cs` |
| **GraphQL** | `Adapters/Primary/GraphQL/Queries/` | `WeatherForecastQuery.cs` |
| **gRPC** | `Adapters/Primary/Grpc/Services/` | `WeatherForecastGrpcService.cs` |
| **CLI** | `Adapters/Primary/Cli/Commands/` | `ForecastCommand.cs` |
| **Repositories** | `Infrastructure/Repositories/` | `EfRepository.cs` |
| **Email Senders** | `Infrastructure/EmailSenders/` | `SmtpEmailSender.cs` |
| **HTTP Clients** | `Infrastructure/Providers/` | `OpenWeatherMapProvider.cs` |
| **Cache** | `Infrastructure/Cache/` | `RedisCacheProvider.cs` |

---

## ✅ Checklist para Agregar Adaptadores

### Adaptador Primario (Driving)

- [ ] Crear carpeta en `Adapters/Primary/{TipoAdaptador}/`
- [ ] Implementar adaptador que dependa de puertos de Domain
- [ ] Registrar en `Program.cs`
- [ ] Probar endpoint/comando
- [ ] Documentar en README

### Adaptador Secundario (Driven)

- [ ] Definir puerto (interface) en `Domain/Ports/`
- [ ] Crear implementación en `Infrastructure/{TipoAdaptador}/`
- [ ] Registrar en `Program.cs`
- [ ] Usar en Application layer
- [ ] Crear tests unitarios

---

## 🎯 Mejores Prácticas

1. **Siempre define el puerto primero** en `Domain/Ports/`
2. **Mantén los adaptadores delgados** - solo conversión de datos
3. **La lógica de negocio va en Application** - no en adaptadores
4. **Usa inyección de dependencias** - nunca `new` en adaptadores
5. **Documenta cada adaptador** - README en cada carpeta
6. **Prueba cada adaptador** - tests de integración

---

## 📚 Recursos Adicionales

- [Hexagonal Architecture](https://alistair.cockburn.us/hexagonal-architecture/)
- [Ports and Adapters](https://herbertograca.com/2017/09/14/ports-adapters-architecture/)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)

---

**¡Listo para agregar cualquier tipo de adaptador!** 🚀
