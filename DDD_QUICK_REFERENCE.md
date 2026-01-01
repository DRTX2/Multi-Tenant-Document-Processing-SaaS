# 🎯 DDD Quick Reference - Referencia Rápida

## 📋 Checklist: Implementar DDD en 10 Pasos

### Fase 1: Fundamentos (1-2 horas)

- [ ] **1. Leer la guía completa** → [DDD_GUIDE.md](DDD_GUIDE.md)
- [ ] **2. Entender la comparación** → [DDD_COMPARISON.md](DDD_COMPARISON.md)
- [ ] **3. Revisar el ejemplo práctico** → [DDD_PRACTICAL_EXAMPLE.md](DDD_PRACTICAL_EXAMPLE.md)

### Fase 2: Implementación Básica (2-3 horas)

- [ ] **4. Crear estructura de carpetas**
  ```bash
  mkdir -p AspNetProject/Domain/Models/ValueObjects
  mkdir -p AspNetProject/Domain/Models/Entities
  mkdir -p AspNetProject/Domain/Models/Enums
  mkdir -p AspNetProject/Domain/Exceptions
  mkdir -p AspNetProject/Domain/Specifications
  ```

- [ ] **5. Crear excepciones de dominio**
  - `Domain/Exceptions/DomainException.cs`
  - `Domain/Exceptions/InvalidForecastDateException.cs`

- [ ] **6. Crear Value Objects**
  - `Domain/Models/ValueObjects/Temperature.cs`
  - `Domain/Models/ValueObjects/Email.cs` (si necesitas)
  - `Domain/Models/ValueObjects/Money.cs` (si necesitas)

### Fase 3: Refactorización (3-4 horas)

- [ ] **7. Refactorizar entidades existentes**
  - Convertir `WeatherForecast` a entidad rica
  - Agregar factory methods
  - Agregar métodos de negocio
  - Hacer setters privados

- [ ] **8. Crear Specifications**
  - `Domain/Specifications/ISpecification.cs`
  - `Domain/Specifications/ValidForecastDateSpecification.cs`
  - `Domain/Specifications/ExtremeTemperatureSpecification.cs`

- [ ] **9. Actualizar servicios**
  - Mover validaciones al dominio
  - Usar specifications
  - Simplificar lógica

### Fase 4: Testing y Documentación (1-2 horas)

- [ ] **10. Crear tests unitarios**
  - Tests de Value Objects
  - Tests de Entidades
  - Tests de Specifications

---

## 🏗️ Building Blocks de DDD - Resumen

### 1️⃣ Value Objects (Objetos de Valor)

**¿Qué son?** Objetos inmutables sin identidad, definidos por sus atributos.

**Cuándo usarlos:**
- ✅ Conceptos del dominio sin identidad única
- ✅ Necesitas validaciones
- ✅ Necesitas conversiones automáticas
- ✅ Quieres inmutabilidad

**Ejemplos:**
```csharp
Temperature.FromCelsius(25)
Email.Create("user@example.com")
Money.Create(100, "USD")
Address.Create("123 Main St", "New York", "NY", "10001", "USA")
```

**Archivo:** `Domain/Models/ValueObjects/Temperature.cs`

---

### 2️⃣ Entities (Entidades)

**¿Qué son?** Objetos con identidad única que persiste en el tiempo.

**Cuándo usarlas:**
- ✅ Tienen un ID único
- ✅ Pueden cambiar de estado
- ✅ Se comparan por ID, no por atributos

**Ejemplo:**
```csharp
var forecast = WeatherForecast.Create(
    date: DateOnly.FromDateTime(DateTime.Today),
    temperature: Temperature.FromCelsius(25),
    condition: WeatherCondition.Sunny,
    city: city
);
```

**Archivo:** `Domain/Models/Entities/WeatherForecast.cs`

---

### 3️⃣ Aggregates (Agregados)

**¿Qué son?** Grupo de entidades relacionadas con una raíz que garantiza consistencia.

**Cuándo usarlos:**
- ✅ Múltiples entidades relacionadas
- ✅ Necesitas garantizar invariantes
- ✅ Se persisten como unidad

**Ejemplo:**
```csharp
var order = Order.Create(customerId);
order.AddItem(productId, "Product", Money.Create(10), 3);
order.Confirm();
// El total SIEMPRE es correcto (invariante)
```

**Archivo:** `Domain/Models/Aggregates/OrderAggregate.cs`

---

### 4️⃣ Domain Events (Eventos de Dominio)

**¿Qué son?** Representan algo que sucedió en el dominio.

**Cuándo usarlos:**
- ✅ Algo importante sucedió en el negocio
- ✅ Necesitas comunicación entre agregados
- ✅ Quieres desacoplar lógica

**Ejemplo:**
```csharp
var orderCreated = new OrderCreatedEvent(
    orderId: order.Id,
    customerId: customer.Id,
    totalAmount: order.Total.Amount
);
await _eventPublisher.PublishAsync(orderCreated);
```

**Archivo:** `Domain/Events/OrderCreatedEvent.cs`

---

### 5️⃣ Specifications (Especificaciones)

**¿Qué son?** Encapsulan reglas de negocio reutilizables.

**Cuándo usarlas:**
- ✅ Reglas de negocio complejas
- ✅ Necesitas reutilizar validaciones
- ✅ Quieres combinar reglas

**Ejemplo:**
```csharp
var validDateSpec = new ValidForecastDateSpecification();
var extremeTempSpec = new ExtremeTemperatureSpecification();

if (!validDateSpec.IsSatisfiedBy(forecast))
    throw new DomainException("Invalid forecast date");

if (extremeTempSpec.IsSatisfiedBy(forecast))
    Console.WriteLine("⚠️ Extreme temperature detected!");
```

**Archivo:** `Domain/Specifications/ValidForecastDateSpecification.cs`

---

### 6️⃣ Domain Services (Servicios de Dominio)

**¿Qué son?** Lógica de dominio que no pertenece a una entidad específica.

**Cuándo usarlos:**
- ✅ Operación sobre múltiples entidades
- ✅ Lógica que no pertenece a una entidad
- ✅ Sin estado

**Ejemplo:**
```csharp
public interface IPricingService
{
    Money CalculateOrderTotal(Order order);
    Money ApplyDiscount(Money price, decimal percentage);
}
```

**Archivo:** `Domain/Services/IPricingService.cs`

---

### 7️⃣ Domain Exceptions (Excepciones de Dominio)

**¿Qué son?** Excepciones específicas del dominio.

**Cuándo usarlas:**
- ✅ Violación de reglas de negocio
- ✅ Estados inválidos
- ✅ Operaciones no permitidas

**Ejemplo:**
```csharp
if (date < DateOnly.FromDateTime(DateTime.Today))
    throw new InvalidForecastDateException("Cannot create forecast for past dates");

if (Status != OrderStatus.Draft)
    throw new DomainException("Cannot add items to non-draft order");
```

**Archivo:** `Domain/Exceptions/DomainException.cs`

---

## 🎨 Patrones de Código

### Factory Method Pattern

```csharp
// ❌ MAL - Constructor público
var forecast = new WeatherForecast
{
    Date = pastDate,  // ¡Puede ser inválido!
    Temperature = -1000  // ¡Sin validación!
};

// ✅ BIEN - Factory Method
var forecast = WeatherForecast.Create(
    date: DateOnly.FromDateTime(DateTime.Today),
    temperature: Temperature.FromCelsius(25),  // Validado
    condition: WeatherCondition.Sunny,
    city: city
);
```

### Encapsulación de Colecciones

```csharp
// ❌ MAL - Lista pública
public class Order
{
    public List<OrderItem> Items { get; set; }  // ¡Puede modificarse directamente!
}

// ✅ BIEN - Colección encapsulada
public class Order
{
    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    
    public void AddItem(OrderItem item)  // Control total
    {
        if (Status != OrderStatus.Draft)
            throw new DomainException("Cannot add items to non-draft order");
        _items.Add(item);
    }
}
```

### Setters Privados

```csharp
// ❌ MAL - Setters públicos
public class WeatherForecast
{
    public DateOnly Date { get; set; }  // ¡Puede cambiarse a cualquier valor!
}

// ✅ BIEN - Setters privados
public class WeatherForecast
{
    public DateOnly Date { get; private set; }
    
    public void UpdateDate(DateOnly newDate)  // Método con validación
    {
        if (newDate < DateOnly.FromDateTime(DateTime.Today))
            throw new DomainException("Cannot set past date");
        Date = newDate;
    }
}
```

---

## 📊 Comparación Rápida

| Aspecto | Sin DDD | Con DDD |
|---------|---------|---------|
| **Modelo** | Anémico (solo datos) | Rico (datos + comportamiento) |
| **Validaciones** | En servicios/controladores | En el dominio |
| **Inmutabilidad** | No garantizada | Garantizada (Value Objects) |
| **Testabilidad** | Difícil | Fácil |
| **Expresividad** | Baja | Alta |
| **Mantenibilidad** | Media | Alta |
| **Complejidad inicial** | Baja | Media-Alta |

---

## 🚀 Comandos Rápidos

### Crear estructura de carpetas
```bash
cd /home/david/RiderProjects/AspNetProject

mkdir -p AspNetProject/Domain/Models/ValueObjects
mkdir -p AspNetProject/Domain/Models/Entities
mkdir -p AspNetProject/Domain/Models/Enums
mkdir -p AspNetProject/Domain/Models/Aggregates
mkdir -p AspNetProject/Domain/Events
mkdir -p AspNetProject/Domain/Services
mkdir -p AspNetProject/Domain/Specifications
mkdir -p AspNetProject/Domain/Exceptions
```

### Compilar
```bash
dotnet build
```

### Ejecutar
```bash
dotnet run --project AspNetProject
```

### Ejecutar tests
```bash
dotnet test
```

---

## 📚 Documentos de Referencia

| Documento | Cuándo Leerlo |
|-----------|---------------|
| [DDD_GUIDE.md](DDD_GUIDE.md) | **Primero** - Guía completa de DDD ⭐⭐⭐ |
| [DDD_PRACTICAL_EXAMPLE.md](DDD_PRACTICAL_EXAMPLE.md) | **Segundo** - Ejemplo paso a paso ⭐⭐ |
| [DDD_COMPARISON.md](DDD_COMPARISON.md) | **Tercero** - Comparación detallada |

---

## 💡 Tips Rápidos

### ✅ DO (Hacer)

1. **Usar Value Objects** para conceptos del dominio
2. **Validar en el dominio**, no en controladores
3. **Setters privados** en entidades
4. **Factory Methods** para crear objetos válidos
5. **Encapsular colecciones** en agregados
6. **Nombres del negocio** (lenguaje ubicuo)

### ❌ DON'T (No Hacer)

1. **No exponer setters públicos**
2. **No lógica de negocio en controladores**
3. **No validaciones solo en UI**
4. **No modelo anémico** (solo getters/setters)
5. **No dependencias de infraestructura en dominio**
6. **No modificar entidades fuera del agregado**

---

## 🎯 Ejemplo Mínimo para Empezar

### 1. Crear Temperature Value Object

```csharp
// Domain/Models/ValueObjects/Temperature.cs
public sealed class Temperature : IEquatable<Temperature>
{
    public double Celsius { get; }
    public double Fahrenheit => (Celsius * 9 / 5) + 32;

    private Temperature(double celsius) => Celsius = celsius;

    public static Temperature FromCelsius(double celsius)
    {
        if (celsius < -273.15)
            throw new DomainException("Temperature below absolute zero");
        return new Temperature(celsius);
    }

    public bool IsHot() => Celsius > 35;
    public bool IsCold() => Celsius < 10;

    public bool Equals(Temperature? other) => 
        other != null && Math.Abs(Celsius - other.Celsius) < 0.01;
    
    public override int GetHashCode() => Celsius.GetHashCode();
}
```

### 2. Usar en WeatherForecast

```csharp
// Domain/Models/Entities/WeatherForecast.cs
public class WeatherForecast : IEntity<Guid>
{
    public Guid Id { get; private set; }
    public DateOnly Date { get; private set; }
    public Temperature Temperature { get; private set; }  // ← Value Object

    public static WeatherForecast Create(DateOnly date, Temperature temperature)
    {
        if (date < DateOnly.FromDateTime(DateTime.Today))
            throw new DomainException("Cannot create forecast for past dates");
        
        return new WeatherForecast
        {
            Id = Guid.NewGuid(),
            Date = date,
            Temperature = temperature
        };
    }

    public bool IsHot() => Temperature.IsHot();
    public bool IsCold() => Temperature.IsCold();
}
```

### 3. Usar en el Servicio

```csharp
// Application/Services/WeatherForecastService.cs
public IEnumerable<WeatherForecast> GetForecasts(int days)
{
    var forecasts = new List<WeatherForecast>();
    
    for (int i = 0; i < days; i++)
    {
        var date = DateOnly.FromDateTime(DateTime.Today.AddDays(i));
        var temp = Temperature.FromCelsius(Random.Shared.Next(-20, 40));
        
        var forecast = WeatherForecast.Create(date, temp);
        forecasts.Add(forecast);
    }
    
    return forecasts;
}
```

---

## 🎓 Próximos Pasos

1. ✅ Lee [DDD_GUIDE.md](DDD_GUIDE.md) - Conceptos completos
2. ✅ Sigue [DDD_PRACTICAL_EXAMPLE.md](DDD_PRACTICAL_EXAMPLE.md) - Implementación paso a paso
3. ✅ Compara en [DDD_COMPARISON.md](DDD_COMPARISON.md) - Antes vs Después
4. ✅ Implementa gradualmente - Empieza con Value Objects
5. ✅ Crea tests unitarios - Valida tu dominio
6. ✅ Itera y mejora - DDD es un viaje, no un destino

---

**¡DDD + Hexagonal = Código de nivel senior!** 🚀
