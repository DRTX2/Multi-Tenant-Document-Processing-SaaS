# 🔄 Comparación: Arquitectura Actual vs. DDD Completo

## 📊 Resumen Ejecutivo

| Aspecto | Actual | Con DDD |
|---------|--------|---------|
| **Modelo de Dominio** | Anémico (solo datos) | Rico (datos + comportamiento) |
| **Validaciones** | En múltiples capas | Centralizadas en el dominio |
| **Expresividad** | Baja | Alta (lenguaje ubicuo) |
| **Testabilidad** | Media | Alta |
| **Mantenibilidad** | Media | Alta |
| **Complejidad Inicial** | Baja | Media-Alta |
| **Escalabilidad** | Media | Alta |

---

## 1. Modelo de Dominio

### ❌ Actual (Modelo Anémico)

```csharp
// Solo contiene datos, sin comportamiento
public class WeatherForecast
{
    public DateOnly Date { get; set; }
    public int TemperatureC { get; set; }
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    public string? Summary { get; set; }
}
```

**Problemas:**
- ❌ No hay validaciones
- ❌ Setters públicos permiten estados inválidos
- ❌ Lógica de negocio dispersa en servicios
- ❌ Difícil de testear
- ❌ No refleja el lenguaje del negocio

### ✅ Con DDD (Modelo Rico)

```csharp
public class WeatherForecast : IEntity<Guid>
{
    public Guid Id { get; private set; }
    public DateOnly Date { get; private set; }
    public Temperature Temperature { get; private set; }  // Value Object
    public WeatherCondition Condition { get; private set; }
    public City City { get; private set; }

    // Factory Method garantiza estado válido
    public static WeatherForecast Create(
        DateOnly date, 
        Temperature temperature, 
        WeatherCondition condition,
        City city)
    {
        if (date < DateOnly.FromDateTime(DateTime.Today))
            throw new InvalidForecastDateException("Cannot create forecast for past dates");

        return new WeatherForecast { ... };
    }

    // Comportamiento del dominio
    public bool IsHot() => Temperature.IsHot();
    public bool RequiresUmbrella() => Condition.RequiresUmbrella();
    public string GetRecommendation() { ... }
}
```

**Ventajas:**
- ✅ Validaciones en el dominio
- ✅ Setters privados garantizan consistencia
- ✅ Lógica de negocio encapsulada
- ✅ Fácil de testear
- ✅ Refleja el lenguaje del negocio

---

## 2. Value Objects

### ❌ Actual (Tipos Primitivos)

```csharp
public class WeatherForecast
{
    public int TemperatureC { get; set; }  // Solo un número
}

// Conversión manual en cada lugar
var fahrenheit = (temperatureC * 9 / 5) + 32;
```

**Problemas:**
- ❌ No hay validaciones (puede ser -1000°C)
- ❌ Conversiones duplicadas en múltiples lugares
- ❌ No encapsula el concepto de "temperatura"
- ❌ Fácil cometer errores

### ✅ Con DDD (Value Object)

```csharp
public sealed class Temperature : IEquatable<Temperature>
{
    public double Celsius { get; }
    public double Fahrenheit => (Celsius * 9 / 5) + 32;
    public double Kelvin => Celsius + 273.15;

    private Temperature(double celsius) => Celsius = celsius;

    public static Temperature FromCelsius(double celsius)
    {
        if (celsius < -273.15)
            throw new DomainException("Temperature cannot be below absolute zero");
        return new Temperature(celsius);
    }

    public bool IsHot() => Celsius > 35;
    public bool IsCold() => Celsius < 10;
}

// Uso
var temp = Temperature.FromCelsius(25);
Console.WriteLine($"{temp.Celsius}°C = {temp.Fahrenheit}°F");
```

**Ventajas:**
- ✅ Validaciones automáticas
- ✅ Conversiones encapsuladas
- ✅ Inmutable (thread-safe)
- ✅ Comportamiento del dominio incluido
- ✅ Reutilizable en toda la aplicación

---

## 3. Validaciones

### ❌ Actual (Dispersas)

```csharp
// En el controlador
[HttpGet]
public IActionResult GetForecasts([FromQuery] int days)
{
    if (days <= 0 || days > 30)
        return BadRequest("Days must be between 1 and 30");
    
    // ...
}

// En el servicio (duplicado)
public IEnumerable<WeatherForecast> GetForecasts(int days)
{
    if (days <= 0 || days > 30)
        throw new ArgumentException("Days must be between 1 and 30");
    
    // ...
}
```

**Problemas:**
- ❌ Validaciones duplicadas
- ❌ Fácil olvidar validar
- ❌ Difícil de mantener
- ❌ No reutilizable

### ✅ Con DDD (Centralizadas)

```csharp
// En el dominio
public class WeatherForecast
{
    public static WeatherForecast Create(DateOnly date, ...)
    {
        if (date < DateOnly.FromDateTime(DateTime.Today))
            throw new InvalidForecastDateException("Cannot create forecast for past dates");
        
        if (date > DateOnly.FromDateTime(DateTime.Today.AddDays(30)))
            throw new InvalidForecastDateException("Cannot forecast more than 30 days in advance");
        
        return new WeatherForecast { ... };
    }
}

// Specifications para reglas complejas
public class ValidForecastDateSpecification : ISpecification<WeatherForecast>
{
    public bool IsSatisfiedBy(WeatherForecast forecast)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        return forecast.Date >= today && forecast.Date <= today.AddDays(30);
    }
}

// En el controlador (sin validaciones de negocio)
[HttpGet]
public IActionResult GetForecasts([FromQuery] int days)
{
    try
    {
        var forecasts = _service.GetForecasts(days);
        return Ok(forecasts);
    }
    catch (DomainException ex)
    {
        return BadRequest(new { error = ex.Message });
    }
}
```

**Ventajas:**
- ✅ Validaciones en un solo lugar
- ✅ Imposible crear objetos inválidos
- ✅ Fácil de testear
- ✅ Reutilizable mediante Specifications

---

## 4. Estructura de Carpetas

### ❌ Actual

```
Domain/
├── Models/
│   ├── City.cs
│   ├── IEntity.cs
│   └── WeatherForecast.cs
└── Ports/
    ├── In/
    └── Out/
```

### ✅ Con DDD

```
Domain/
├── Models/
│   ├── Entities/              # Entidades con identidad
│   │   ├── WeatherForecast.cs
│   │   └── Customer.cs
│   ├── ValueObjects/          # Objetos inmutables
│   │   ├── Temperature.cs
│   │   ├── Email.cs
│   │   ├── Money.cs
│   │   └── Address.cs
│   ├── Aggregates/            # Raíces de agregados
│   │   └── OrderAggregate.cs
│   ├── Enums/                 # Enumeraciones ricas
│   │   ├── WeatherCondition.cs
│   │   └── OrderStatus.cs
│   └── IEntity.cs
├── Events/                    # Eventos de dominio
│   ├── IDomainEvent.cs
│   ├── OrderCreatedEvent.cs
│   └── WeatherAlertEvent.cs
├── Services/                  # Servicios de dominio
│   ├── IPricingService.cs
│   └── IWeatherValidationService.cs
├── Specifications/            # Reglas de negocio
│   ├── ISpecification.cs
│   ├── ValidForecastDateSpecification.cs
│   └── ExtremeTemperatureSpecification.cs
├── Exceptions/                # Excepciones del dominio
│   ├── DomainException.cs
│   ├── InvalidEmailException.cs
│   └── InvalidForecastDateException.cs
└── Ports/
    ├── In/
    └── Out/
```

---

## 5. Ejemplo Completo: Crear una Orden

### ❌ Actual (Sin DDD)

```csharp
// Modelo anémico
public class Order
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public List<OrderItem> Items { get; set; }
    public decimal Total { get; set; }
    public string Status { get; set; }
}

// Servicio con toda la lógica
public class OrderService
{
    public async Task<Guid> CreateOrder(Guid customerId, List<OrderItemDto> items)
    {
        // Validaciones dispersas
        if (items == null || !items.Any())
            throw new Exception("Order must have items");
        
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Items = new List<OrderItem>(),
            Status = "Draft"
        };
        
        decimal total = 0;
        foreach (var item in items)
        {
            // Validación manual
            if (item.Quantity <= 0)
                throw new Exception("Quantity must be positive");
            
            var orderItem = new OrderItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            };
            
            order.Items.Add(orderItem);
            total += item.Quantity * item.UnitPrice;
        }
        
        order.Total = total;
        
        await _repository.AddAsync(order);
        return order.Id;
    }
}
```

**Problemas:**
- ❌ Lógica de negocio en el servicio
- ❌ Validaciones manuales
- ❌ Fácil olvidar actualizar el total
- ❌ Estado puede ser inconsistente
- ❌ Difícil de testear

### ✅ Con DDD

```csharp
// Agregado rico
public class Order : IEntity<Guid>
{
    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    
    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    
    // El total SIEMPRE es correcto (invariante)
    public Money Total => _items
        .Select(i => i.Subtotal)
        .Aggregate(Money.Create(0), (acc, money) => acc.Add(money));

    public static Order Create(Guid customerId)
    {
        return new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Status = OrderStatus.Draft
        };
    }

    // Métodos que garantizan invariantes
    public void AddItem(Guid productId, string productName, Money unitPrice, int quantity)
    {
        if (Status != OrderStatus.Draft)
            throw new DomainException("Cannot add items to non-draft order");
        
        if (quantity <= 0)
            throw new DomainException("Quantity must be positive");
        
        var existingItem = _items.FirstOrDefault(i => i.ProductId == productId);
        if (existingItem != null)
        {
            existingItem.IncreaseQuantity(quantity);
        }
        else
        {
            _items.Add(OrderItem.Create(productId, productName, unitPrice, quantity));
        }
    }

    public void Confirm()
    {
        if (Status != OrderStatus.Draft)
            throw new DomainException("Only draft orders can be confirmed");
        
        if (!_items.Any())
            throw new DomainException("Cannot confirm order without items");
        
        Status = OrderStatus.Confirmed;
    }
}

// Servicio simplificado
public class OrderService : IOrderService
{
    private readonly IRepository<Order, Guid> _orderRepository;
    private readonly IEventPublisher _eventPublisher;

    public async Task<Guid> CreateOrderAsync(Guid customerId, List<OrderItemDto> items)
    {
        // 1. Crear agregado
        var order = Order.Create(customerId);
        
        // 2. Agregar items (validaciones en el agregado)
        foreach (var item in items)
        {
            var price = Money.Create(item.UnitPrice);
            order.AddItem(item.ProductId, item.ProductName, price, item.Quantity);
        }
        
        // 3. Validar con specification
        var isValid = new OrderIsValidSpecification();
        if (!isValid.IsSatisfiedBy(order))
            throw new DomainException("Order is not valid");
        
        // 4. Persistir
        await _orderRepository.AddAsync(order);
        
        // 5. Publicar evento
        await _eventPublisher.PublishAsync(
            new OrderCreatedEvent(order.Id, customerId, order.Total.Amount));
        
        return order.Id;
    }
}
```

**Ventajas:**
- ✅ Lógica de negocio en el dominio
- ✅ Validaciones automáticas
- ✅ Total siempre correcto (invariante)
- ✅ Estado siempre consistente
- ✅ Fácil de testear
- ✅ Servicio más simple

---

## 6. Testing

### ❌ Actual (Difícil)

```csharp
// Necesitas mockear muchas cosas
[Fact]
public async Task CreateOrder_ShouldCalculateTotal()
{
    // Arrange
    var mockRepo = new Mock<IRepository<Order>>();
    var service = new OrderService(mockRepo.Object);
    
    // Act
    var orderId = await service.CreateOrder(customerId, items);
    
    // Assert - difícil verificar el total
    mockRepo.Verify(r => r.AddAsync(It.Is<Order>(o => o.Total == expectedTotal)));
}
```

### ✅ Con DDD (Fácil)

```csharp
// Tests unitarios del dominio (sin mocks)
[Fact]
public void Order_AddItem_ShouldCalculateTotalCorrectly()
{
    // Arrange
    var order = Order.Create(Guid.NewGuid());
    var price = Money.Create(10);
    
    // Act
    order.AddItem(productId, "Product", price, 3);
    
    // Assert
    Assert.Equal(30, order.Total.Amount);
}

[Fact]
public void Order_AddItem_ToConfirmedOrder_ShouldThrowException()
{
    // Arrange
    var order = Order.Create(Guid.NewGuid());
    order.AddItem(productId, "Product", Money.Create(10), 1);
    order.Confirm();
    
    // Act & Assert
    Assert.Throws<DomainException>(() => 
        order.AddItem(productId2, "Product2", Money.Create(10), 1));
}

[Fact]
public void Temperature_FromCelsius_BelowAbsoluteZero_ShouldThrowException()
{
    // Act & Assert
    Assert.Throws<DomainException>(() => Temperature.FromCelsius(-300));
}

[Fact]
public void ValidForecastDateSpecification_PastDate_ShouldReturnFalse()
{
    // Arrange
    var spec = new ValidForecastDateSpecification();
    var pastDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));
    var forecast = WeatherForecast.Create(
        pastDate, 
        Temperature.FromCelsius(20), 
        WeatherCondition.Sunny,
        city);
    
    // Act
    var result = spec.IsSatisfiedBy(forecast);
    
    // Assert
    Assert.False(result);
}
```

---

## 7. Cuándo Usar DDD

### ✅ Usa DDD cuando:

- ✅ El dominio es **complejo** y tiene muchas reglas de negocio
- ✅ El proyecto es **grande** y de **larga duración**
- ✅ Tienes acceso a **expertos del dominio**
- ✅ Las reglas de negocio **cambian frecuentemente**
- ✅ Necesitas **alta testabilidad**
- ✅ El equipo está **dispuesto a aprender** DDD

**Ejemplos:** E-commerce, Banking, Healthcare, Insurance

### ❌ No uses DDD cuando:

- ❌ El dominio es **simple** (CRUD básico)
- ❌ El proyecto es **pequeño** o **temporal**
- ❌ No hay **reglas de negocio complejas**
- ❌ El equipo no tiene **tiempo para aprender**
- ❌ Es un **prototipo** o **MVP rápido**

**Ejemplos:** Blog simple, Landing page, Admin panel básico

---

## 8. Migración Gradual

No necesitas migrar todo de una vez. Puedes hacerlo gradualmente:

### Fase 1: Value Objects (Bajo riesgo)
```csharp
// Reemplazar primitivos por Value Objects
public class WeatherForecast
{
    public Temperature Temperature { get; set; }  // En lugar de int
    public Email ContactEmail { get; set; }       // En lugar de string
}
```

### Fase 2: Entidades Ricas (Medio riesgo)
```csharp
// Agregar comportamiento a las entidades
public class WeatherForecast
{
    public bool IsHot() => Temperature.IsHot();
    public string GetRecommendation() { ... }
}
```

### Fase 3: Aggregates (Medio-Alto riesgo)
```csharp
// Encapsular colecciones y garantizar invariantes
public class Order
{
    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    
    public void AddItem(...) { ... }  // Controla cómo se agregan items
}
```

### Fase 4: Domain Events (Alto riesgo)
```csharp
// Comunicación mediante eventos
public class Order
{
    public void Confirm()
    {
        Status = OrderStatus.Confirmed;
        RaiseDomainEvent(new OrderConfirmedEvent(Id));
    }
}
```

---

## 9. Checklist de Implementación

### Para tu proyecto actual:

- [ ] **Paso 1:** Crear `Temperature` Value Object
- [ ] **Paso 2:** Crear `WeatherCondition` enum rico
- [ ] **Paso 3:** Crear excepciones de dominio
- [ ] **Paso 4:** Refactorizar `WeatherForecast` como entidad rica
- [ ] **Paso 5:** Crear `ValidForecastDateSpecification`
- [ ] **Paso 6:** Mover validaciones al dominio
- [ ] **Paso 7:** Actualizar servicios para usar el nuevo modelo
- [ ] **Paso 8:** Actualizar controladores
- [ ] **Paso 9:** Crear tests unitarios del dominio
- [ ] **Paso 10:** Documentar el lenguaje ubicuo

---

## 10. Conclusión

### Resumen de Beneficios

| Beneficio | Impacto |
|-----------|---------|
| **Validaciones centralizadas** | ⭐⭐⭐⭐⭐ |
| **Testabilidad** | ⭐⭐⭐⭐⭐ |
| **Expresividad del código** | ⭐⭐⭐⭐⭐ |
| **Mantenibilidad** | ⭐⭐⭐⭐⭐ |
| **Escalabilidad** | ⭐⭐⭐⭐ |
| **Curva de aprendizaje** | ⭐⭐ (requiere estudio) |
| **Complejidad inicial** | ⭐⭐⭐ (más código al inicio) |

### Recomendación

Para tu proyecto actual de **WeatherForecast**:

1. **Empieza con Value Objects** (`Temperature`) - Bajo riesgo, alto beneficio
2. **Agrega comportamiento a entidades** - Medio riesgo, alto beneficio
3. **Usa Specifications** para reglas complejas - Bajo riesgo, medio beneficio
4. **Considera Domain Events** solo si creces - Alto riesgo, beneficio a largo plazo

**DDD + Arquitectura Hexagonal = 🚀 Código de nivel senior**
