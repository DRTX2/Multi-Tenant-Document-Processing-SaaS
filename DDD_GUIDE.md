# 🏛️ Domain-Driven Design (DDD) - Guía de Implementación

## 📚 Índice
1. [Introducción](#introducción)
2. [Conceptos Clave de DDD](#conceptos-clave-de-ddd)
3. [Estructura de Carpetas](#estructura-de-carpetas)
4. [Building Blocks de DDD](#building-blocks-de-ddd)
5. [Ejemplos Prácticos](#ejemplos-prácticos)
6. [Integración con Arquitectura Hexagonal](#integración-con-arquitectura-hexagonal)
7. [Mejores Prácticas](#mejores-prácticas)

---

## Introducción

**Domain-Driven Design (DDD)** es un enfoque de desarrollo de software que pone el **dominio del negocio** en el centro de la aplicación. Se complementa perfectamente con la **Arquitectura Hexagonal**.

### ¿Por qué DDD?

- ✅ **Lenguaje Ubicuo**: Desarrolladores y expertos del negocio hablan el mismo idioma
- ✅ **Modelo Rico**: El dominio contiene lógica de negocio, no solo datos
- ✅ **Mantenibilidad**: Código que refleja el negocio es más fácil de entender
- ✅ **Escalabilidad**: Bounded Contexts permiten dividir sistemas complejos

---

## Conceptos Clave de DDD

### 1. **Ubiquitous Language** (Lenguaje Ubicuo)
Vocabulario compartido entre desarrolladores y expertos del dominio.

**Ejemplo:**
```csharp
// ❌ MAL - Lenguaje técnico
public class UserData { public int Status { get; set; } }

// ✅ BIEN - Lenguaje del negocio
public class Customer { public CustomerStatus Status { get; set; } }
public enum CustomerStatus { Active, Suspended, Inactive }
```

### 2. **Bounded Context** (Contexto Delimitado)
Límites explícitos donde un modelo de dominio es válido.

**Ejemplo:**
- **Contexto de Ventas**: `Order`, `Customer`, `Product`
- **Contexto de Inventario**: `Stock`, `Warehouse`, `Product`
- El `Product` significa cosas diferentes en cada contexto

### 3. **Entities** (Entidades)
Objetos con identidad única que persiste en el tiempo.

### 4. **Value Objects** (Objetos de Valor)
Objetos inmutables sin identidad, definidos por sus atributos.

### 5. **Aggregates** (Agregados)
Grupo de entidades y value objects con una raíz que garantiza consistencia.

### 6. **Domain Events** (Eventos de Dominio)
Algo que sucedió en el dominio que es importante para el negocio.

### 7. **Domain Services** (Servicios de Dominio)
Operaciones del dominio que no pertenecen a una entidad específica.

### 8. **Repositories** (Repositorios)
Abstracciones para persistir y recuperar agregados.

---

## Estructura de Carpetas

```
Domain/
├── Models/                          # Entidades y Value Objects
│   ├── Entities/                    # Entidades del dominio
│   │   ├── Customer.cs
│   │   ├── Order.cs
│   │   └── OrderItem.cs
│   ├── ValueObjects/                # Objetos de valor
│   │   ├── Address.cs
│   │   ├── Email.cs
│   │   ├── Money.cs
│   │   └── Temperature.cs
│   ├── Aggregates/                  # Raíces de agregados
│   │   └── OrderAggregate.cs
│   ├── Enums/                       # Enumeraciones del dominio
│   │   ├── OrderStatus.cs
│   │   └── PaymentMethod.cs
│   └── IEntity.cs                   # Interfaz base
├── Events/                          # Eventos de dominio
│   ├── OrderCreatedEvent.cs
│   ├── OrderCancelledEvent.cs
│   └── IDomainEvent.cs
├── Services/                        # Servicios de dominio
│   ├── IPricingService.cs
│   └── IOrderValidationService.cs
├── Specifications/                  # Especificaciones
│   ├── OrderIsValidSpecification.cs
│   └── ISpecification.cs
├── Exceptions/                      # Excepciones del dominio
│   ├── DomainException.cs
│   ├── InvalidEmailException.cs
│   └── OrderCannotBeCancelledException.cs
└── Ports/                           # Puertos (Hexagonal Architecture)
    ├── In/                          # Casos de uso
    └── Out/                         # Dependencias externas
```

---

## Building Blocks de DDD

### 1. Entities (Entidades)

**Características:**
- Tienen identidad única (ID)
- Pueden cambiar de estado
- Se comparan por ID, no por atributos

**Ejemplo:**

```csharp
// Domain/Models/Entities/Customer.cs
namespace AspNetProject.Domain.Models.Entities;

/// <summary>
/// Entidad Customer - Tiene identidad única
/// </summary>
public class Customer : IEntity<Guid>
{
    public Guid Id { get; private set; }
    public Email Email { get; private set; }
    public string Name { get; private set; }
    public Address ShippingAddress { get; private set; }
    public CustomerStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastModifiedAt { get; private set; }

    // Constructor privado - solo se crea mediante factory method
    private Customer() { }

    // Factory Method
    public static Customer Create(Email email, string name, Address address)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Customer name cannot be empty");

        return new Customer
        {
            Id = Guid.NewGuid(),
            Email = email,
            Name = name,
            ShippingAddress = address,
            Status = CustomerStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
    }

    // Métodos de negocio
    public void UpdateAddress(Address newAddress)
    {
        if (Status == CustomerStatus.Inactive)
            throw new DomainException("Cannot update address of inactive customer");

        ShippingAddress = newAddress;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void Suspend()
    {
        if (Status == CustomerStatus.Inactive)
            throw new DomainException("Cannot suspend inactive customer");

        Status = CustomerStatus.Suspended;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        Status = CustomerStatus.Active;
        LastModifiedAt = DateTime.UtcNow;
    }
}

public enum CustomerStatus
{
    Active,
    Suspended,
    Inactive
}
```

---

### 2. Value Objects (Objetos de Valor)

**Características:**
- Inmutables
- Sin identidad
- Se comparan por valor
- Encapsulan validaciones

**Ejemplos:**

#### Email Value Object

```csharp
// Domain/Models/ValueObjects/Email.cs
namespace AspNetProject.Domain.Models.ValueObjects;

/// <summary>
/// Value Object para Email - Inmutable y con validación
/// </summary>
public sealed class Email : IEquatable<Email>
{
    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new InvalidEmailException("Email cannot be empty");

        if (!IsValidEmail(email))
            throw new InvalidEmailException($"Invalid email format: {email}");

        return new Email(email.ToLowerInvariant());
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    // Equality por valor
    public bool Equals(Email? other)
    {
        if (other is null) return false;
        return Value == other.Value;
    }

    public override bool Equals(object? obj) => Equals(obj as Email);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value;

    public static implicit operator string(Email email) => email.Value;
}
```

#### Money Value Object

```csharp
// Domain/Models/ValueObjects/Money.cs
namespace AspNetProject.Domain.Models.ValueObjects;

/// <summary>
/// Value Object para Money - Maneja moneda y cantidad
/// </summary>
public sealed class Money : IEquatable<Money>
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Create(decimal amount, string currency = "USD")
    {
        if (amount < 0)
            throw new DomainException("Money amount cannot be negative");

        if (string.IsNullOrWhiteSpace(currency))
            throw new DomainException("Currency cannot be empty");

        return new Money(amount, currency.ToUpperInvariant());
    }

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new DomainException($"Cannot add {other.Currency} to {Currency}");

        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        if (Currency != other.Currency)
            throw new DomainException($"Cannot subtract {other.Currency} from {Currency}");

        if (Amount < other.Amount)
            throw new DomainException("Insufficient funds");

        return new Money(Amount - other.Amount, Currency);
    }

    public Money Multiply(decimal factor)
    {
        if (factor < 0)
            throw new DomainException("Factor cannot be negative");

        return new Money(Amount * factor, Currency);
    }

    public bool Equals(Money? other)
    {
        if (other is null) return false;
        return Amount == other.Amount && Currency == other.Currency;
    }

    public override bool Equals(object? obj) => Equals(obj as Money);
    public override int GetHashCode() => HashCode.Combine(Amount, Currency);
    public override string ToString() => $"{Amount:F2} {Currency}";

    public static bool operator >(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new DomainException("Cannot compare different currencies");
        return left.Amount > right.Amount;
    }

    public static bool operator <(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new DomainException("Cannot compare different currencies");
        return left.Amount < right.Amount;
    }
}
```

#### Address Value Object

```csharp
// Domain/Models/ValueObjects/Address.cs
namespace AspNetProject.Domain.Models.ValueObjects;

/// <summary>
/// Value Object para Address - Inmutable
/// </summary>
public sealed class Address : IEquatable<Address>
{
    public string Street { get; }
    public string City { get; }
    public string State { get; }
    public string ZipCode { get; }
    public string Country { get; }

    private Address(string street, string city, string state, string zipCode, string country)
    {
        Street = street;
        City = city;
        State = state;
        ZipCode = zipCode;
        Country = country;
    }

    public static Address Create(string street, string city, string state, string zipCode, string country)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new DomainException("Street cannot be empty");
        if (string.IsNullOrWhiteSpace(city))
            throw new DomainException("City cannot be empty");
        if (string.IsNullOrWhiteSpace(zipCode))
            throw new DomainException("ZipCode cannot be empty");
        if (string.IsNullOrWhiteSpace(country))
            throw new DomainException("Country cannot be empty");

        return new Address(street, city, state, zipCode, country);
    }

    public bool Equals(Address? other)
    {
        if (other is null) return false;
        return Street == other.Street &&
               City == other.City &&
               State == other.State &&
               ZipCode == other.ZipCode &&
               Country == other.Country;
    }

    public override bool Equals(object? obj) => Equals(obj as Address);
    
    public override int GetHashCode() => 
        HashCode.Combine(Street, City, State, ZipCode, Country);

    public override string ToString() => 
        $"{Street}, {City}, {State} {ZipCode}, {Country}";
}
```

#### Temperature Value Object (para WeatherForecast)

```csharp
// Domain/Models/ValueObjects/Temperature.cs
namespace AspNetProject.Domain.Models.ValueObjects;

/// <summary>
/// Value Object para Temperature - Maneja conversiones
/// </summary>
public sealed class Temperature : IEquatable<Temperature>
{
    public double Celsius { get; }
    public double Fahrenheit => (Celsius * 9 / 5) + 32;
    public double Kelvin => Celsius + 273.15;

    private Temperature(double celsius)
    {
        Celsius = celsius;
    }

    public static Temperature FromCelsius(double celsius)
    {
        if (celsius < -273.15)
            throw new DomainException("Temperature cannot be below absolute zero");

        return new Temperature(celsius);
    }

    public static Temperature FromFahrenheit(double fahrenheit)
    {
        var celsius = (fahrenheit - 32) * 5 / 9;
        return FromCelsius(celsius);
    }

    public static Temperature FromKelvin(double kelvin)
    {
        if (kelvin < 0)
            throw new DomainException("Kelvin cannot be negative");

        return FromCelsius(kelvin - 273.15);
    }

    public bool Equals(Temperature? other)
    {
        if (other is null) return false;
        return Math.Abs(Celsius - other.Celsius) < 0.01;
    }

    public override bool Equals(object? obj) => Equals(obj as Temperature);
    public override int GetHashCode() => Celsius.GetHashCode();
    public override string ToString() => $"{Celsius:F1}°C ({Fahrenheit:F1}°F)";
}
```

---

### 3. Aggregates (Agregados)

**Características:**
- Grupo de entidades relacionadas
- Una raíz de agregado (Aggregate Root)
- Garantiza invariantes de negocio
- Se persiste y recupera como unidad

**Ejemplo: Order Aggregate**

```csharp
// Domain/Models/Aggregates/OrderAggregate.cs
namespace AspNetProject.Domain.Models.Aggregates;

/// <summary>
/// Order Aggregate Root - Garantiza consistencia del pedido
/// </summary>
public class Order : IEntity<Guid>
{
    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    // Invariante: El total siempre es la suma de los items
    public Money Total => _items
        .Select(i => i.Subtotal)
        .Aggregate(Money.Create(0), (acc, money) => acc.Add(money));

    private Order() { }

    public static Order Create(Guid customerId)
    {
        return new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Status = OrderStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };
    }

    // Métodos que garantizan invariantes
    public void AddItem(Guid productId, string productName, Money unitPrice, int quantity)
    {
        if (Status != OrderStatus.Draft)
            throw new DomainException("Cannot add items to a non-draft order");

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

    public void RemoveItem(Guid productId)
    {
        if (Status != OrderStatus.Draft)
            throw new DomainException("Cannot remove items from a non-draft order");

        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item == null)
            throw new DomainException("Item not found in order");

        _items.Remove(item);
    }

    public void Confirm()
    {
        if (Status != OrderStatus.Draft)
            throw new DomainException("Only draft orders can be confirmed");

        if (!_items.Any())
            throw new DomainException("Cannot confirm order without items");

        Status = OrderStatus.Confirmed;
    }

    public void Complete()
    {
        if (Status != OrderStatus.Confirmed)
            throw new DomainException("Only confirmed orders can be completed");

        Status = OrderStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Completed)
            throw new DomainException("Cannot cancel completed order");

        Status = OrderStatus.Cancelled;
    }
}

/// <summary>
/// OrderItem - Entidad dentro del agregado Order
/// </summary>
public class OrderItem
{
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; }
    public Money UnitPrice { get; private set; }
    public int Quantity { get; private set; }
    public Money Subtotal => UnitPrice.Multiply(Quantity);

    private OrderItem() { }

    internal static OrderItem Create(Guid productId, string productName, Money unitPrice, int quantity)
    {
        return new OrderItem
        {
            ProductId = productId,
            ProductName = productName,
            UnitPrice = unitPrice,
            Quantity = quantity
        };
    }

    internal void IncreaseQuantity(int amount)
    {
        if (amount <= 0)
            throw new DomainException("Amount must be positive");

        Quantity += amount;
    }

    internal void DecreaseQuantity(int amount)
    {
        if (amount <= 0)
            throw new DomainException("Amount must be positive");

        if (Quantity - amount < 0)
            throw new DomainException("Insufficient quantity");

        Quantity -= amount;
    }
}

public enum OrderStatus
{
    Draft,
    Confirmed,
    Completed,
    Cancelled
}
```

---

### 4. Domain Events (Eventos de Dominio)

**Características:**
- Representan algo que sucedió
- Inmutables
- Pasado (OrderCreated, CustomerRegistered)

**Ejemplos:**

```csharp
// Domain/Events/IDomainEvent.cs
namespace AspNetProject.Domain.Events;

/// <summary>
/// Interfaz base para eventos de dominio
/// </summary>
public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredOn { get; }
}
```

```csharp
// Domain/Events/OrderCreatedEvent.cs
namespace AspNetProject.Domain.Events;

/// <summary>
/// Evento: Se creó una orden
/// </summary>
public sealed class OrderCreatedEvent : IDomainEvent
{
    public Guid EventId { get; }
    public DateTime OccurredOn { get; }
    public Guid OrderId { get; }
    public Guid CustomerId { get; }
    public decimal TotalAmount { get; }

    public OrderCreatedEvent(Guid orderId, Guid customerId, decimal totalAmount)
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        OrderId = orderId;
        CustomerId = customerId;
        TotalAmount = totalAmount;
    }
}
```

```csharp
// Domain/Events/OrderCancelledEvent.cs
namespace AspNetProject.Domain.Events;

public sealed class OrderCancelledEvent : IDomainEvent
{
    public Guid EventId { get; }
    public DateTime OccurredOn { get; }
    public Guid OrderId { get; }
    public string Reason { get; }

    public OrderCancelledEvent(Guid orderId, string reason)
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        OrderId = orderId;
        Reason = reason;
    }
}
```

---

### 5. Domain Services (Servicios de Dominio)

**Características:**
- Lógica de dominio que no pertenece a una entidad
- Operan sobre múltiples entidades
- Sin estado

**Ejemplo:**

```csharp
// Domain/Services/IPricingService.cs
namespace AspNetProject.Domain.Services;

/// <summary>
/// Servicio de Dominio - Cálculo de precios
/// </summary>
public interface IPricingService
{
    Money CalculateOrderTotal(Order order);
    Money ApplyDiscount(Money originalPrice, decimal discountPercentage);
}
```

```csharp
// Application/Services/PricingService.cs (implementación)
namespace AspNetProject.Application.Services;

public class PricingService : IPricingService
{
    public Money CalculateOrderTotal(Order order)
    {
        var subtotal = order.Items
            .Select(i => i.Subtotal)
            .Aggregate(Money.Create(0), (acc, money) => acc.Add(money));

        // Aplicar descuentos, impuestos, etc.
        return subtotal;
    }

    public Money ApplyDiscount(Money originalPrice, decimal discountPercentage)
    {
        if (discountPercentage < 0 || discountPercentage > 100)
            throw new DomainException("Discount percentage must be between 0 and 100");

        var discountAmount = originalPrice.Multiply(discountPercentage / 100);
        return originalPrice.Subtract(discountAmount);
    }
}
```

---

### 6. Specifications (Especificaciones)

**Características:**
- Encapsulan reglas de negocio
- Reutilizables
- Combinables

**Ejemplo:**

```csharp
// Domain/Specifications/ISpecification.cs
namespace AspNetProject.Domain.Specifications;

/// <summary>
/// Patrón Specification - Encapsula reglas de negocio
/// </summary>
public interface ISpecification<T>
{
    bool IsSatisfiedBy(T entity);
}
```

```csharp
// Domain/Specifications/OrderIsValidSpecification.cs
namespace AspNetProject.Domain.Specifications;

public class OrderIsValidSpecification : ISpecification<Order>
{
    public bool IsSatisfiedBy(Order order)
    {
        return order.Items.Any() && 
               order.Total.Amount > 0 &&
               order.Status == OrderStatus.Draft;
    }
}
```

```csharp
// Domain/Specifications/CustomerCanPlaceOrderSpecification.cs
namespace AspNetProject.Domain.Specifications;

public class CustomerCanPlaceOrderSpecification : ISpecification<Customer>
{
    public bool IsSatisfiedBy(Customer customer)
    {
        return customer.Status == CustomerStatus.Active &&
               customer.Email != null;
    }
}
```

---

### 7. Domain Exceptions (Excepciones de Dominio)

```csharp
// Domain/Exceptions/DomainException.cs
namespace AspNetProject.Domain.Exceptions;

/// <summary>
/// Excepción base del dominio
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
    
    public DomainException(string message, Exception innerException) 
        : base(message, innerException) { }
}
```

```csharp
// Domain/Exceptions/InvalidEmailException.cs
namespace AspNetProject.Domain.Exceptions;

public class InvalidEmailException : DomainException
{
    public InvalidEmailException(string message) : base(message) { }
}
```

---

## Integración con Arquitectura Hexagonal

### Cómo DDD y Hexagonal se Complementan

```
┌─────────────────────────────────────────────────────────────┐
│                    HEXAGONAL + DDD                          │
└─────────────────────────────────────────────────────────────┘

Adapters/In/                    Domain/                    Infrastructure/
(Driving)                       (Core - DDD)               (Driven)
┌──────────────┐               ┌──────────────┐           ┌──────────────┐
│              │               │ Entities     │           │              │
│ Controllers  │──────────────▶│ Value Objects│           │ Repositories │
│ GraphQL      │               │ Aggregates   │◀──────────│ Providers    │
│ gRPC         │               │ Events       │           │ External APIs│
│              │               │ Services     │           │              │
└──────────────┘               │ Specs        │           └──────────────┘
                               └──────────────┘
                                      ▲
                                      │
                               ┌──────────────┐
                               │ Application  │
                               │ Services     │
                               │ (Use Cases)  │
                               └──────────────┘
```

### Ejemplo Completo: Crear una Orden

#### 1. Puerto de Entrada (Use Case)

```csharp
// Domain/Ports/In/IOrderService.cs
namespace AspNetProject.Domain.Ports.In;

public interface IOrderService
{
    Task<Guid> CreateOrderAsync(Guid customerId, List<OrderItemDto> items);
    Task ConfirmOrderAsync(Guid orderId);
    Task CancelOrderAsync(Guid orderId, string reason);
}
```

#### 2. Implementación del Caso de Uso

```csharp
// Application/Services/OrderService.cs
namespace AspNetProject.Application.Services;

public class OrderService : IOrderService
{
    private readonly IRepository<Order, Guid> _orderRepository;
    private readonly IRepository<Customer, Guid> _customerRepository;
    private readonly IPricingService _pricingService;
    private readonly IEventPublisher _eventPublisher;

    public OrderService(
        IRepository<Order, Guid> orderRepository,
        IRepository<Customer, Guid> customerRepository,
        IPricingService pricingService,
        IEventPublisher eventPublisher)
    {
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
        _pricingService = pricingService;
        _eventPublisher = eventPublisher;
    }

    public async Task<Guid> CreateOrderAsync(Guid customerId, List<OrderItemDto> items)
    {
        // 1. Validar que el cliente existe y puede hacer pedidos
        var customer = await _customerRepository.GetByIdAsync(customerId);
        if (customer == null)
            throw new DomainException("Customer not found");

        var canPlaceOrder = new CustomerCanPlaceOrderSpecification();
        if (!canPlaceOrder.IsSatisfiedBy(customer))
            throw new DomainException("Customer cannot place orders");

        // 2. Crear el agregado Order
        var order = Order.Create(customerId);

        // 3. Agregar items
        foreach (var item in items)
        {
            var price = Money.Create(item.UnitPrice);
            order.AddItem(item.ProductId, item.ProductName, price, item.Quantity);
        }

        // 4. Validar el pedido
        var isValid = new OrderIsValidSpecification();
        if (!isValid.IsSatisfiedBy(order))
            throw new DomainException("Order is not valid");

        // 5. Persistir
        await _orderRepository.AddAsync(order);

        // 6. Publicar evento de dominio
        var orderCreatedEvent = new OrderCreatedEvent(
            order.Id, 
            customerId, 
            order.Total.Amount);
        await _eventPublisher.PublishAsync(orderCreatedEvent);

        return order.Id;
    }

    public async Task ConfirmOrderAsync(Guid orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
            throw new DomainException("Order not found");

        order.Confirm();
        await _orderRepository.UpdateAsync(order);
    }

    public async Task CancelOrderAsync(Guid orderId, string reason)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
            throw new DomainException("Order not found");

        order.Cancel();
        await _orderRepository.UpdateAsync(order);

        var cancelledEvent = new OrderCancelledEvent(orderId, reason);
        await _eventPublisher.PublishAsync(cancelledEvent);
    }
}
```

#### 3. Controlador (Adaptador de Entrada)

```csharp
// Adapters/In/Rest/Controllers/OrderController.cs
namespace AspNetProject.Adapters.In.Rest.Controllers;

[ApiController]
[Route("api/orders")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        try
        {
            var orderId = await _orderService.CreateOrderAsync(
                request.CustomerId, 
                request.Items);

            return CreatedAtAction(nameof(GetOrder), new { id = orderId }, orderId);
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id}/confirm")]
    public async Task<IActionResult> ConfirmOrder(Guid id)
    {
        try
        {
            await _orderService.ConfirmOrderAsync(id);
            return NoContent();
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
```

---

## Mejores Prácticas

### ✅ DO (Hacer)

1. **Usar Lenguaje Ubicuo**: Nombres de clases y métodos deben reflejar el negocio
2. **Value Objects para conceptos del dominio**: Email, Money, Temperature
3. **Validaciones en el dominio**: No en controladores o DTOs
4. **Inmutabilidad en Value Objects**: Siempre crear nuevos objetos
5. **Factory Methods**: Para crear entidades con estado válido
6. **Encapsular colecciones**: Usar `IReadOnlyCollection` en agregados
7. **Eventos de dominio**: Para comunicar cambios importantes

### ❌ DON'T (No Hacer)

1. **No exponer setters públicos**: Usar métodos de negocio
2. **No lógica de negocio en controladores**: Debe estar en el dominio
3. **No anemic domain model**: Entidades con solo getters/setters
4. **No dependencias de infraestructura en el dominio**: Mantener puro
5. **No validaciones solo en UI**: Siempre validar en el dominio
6. **No modificar entidades fuera del agregado**: Usar la raíz

---

## Ejemplo Práctico: Refactorizar WeatherForecast con DDD

### Antes (Modelo Anémico)

```csharp
public class WeatherForecast
{
    public DateOnly Date { get; set; }
    public int TemperatureC { get; set; }
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    public string? Summary { get; set; }
}
```

### Después (Modelo Rico con DDD)

```csharp
// Domain/Models/Entities/WeatherForecast.cs
namespace AspNetProject.Domain.Models.Entities;

/// <summary>
/// Entidad WeatherForecast - Modelo rico con DDD
/// </summary>
public class WeatherForecast : IEntity<Guid>
{
    public Guid Id { get; private set; }
    public DateOnly Date { get; private set; }
    public Temperature Temperature { get; private set; }
    public WeatherCondition Condition { get; private set; }
    public City City { get; private set; }

    private WeatherForecast() { }

    public static WeatherForecast Create(
        DateOnly date, 
        Temperature temperature, 
        WeatherCondition condition,
        City city)
    {
        if (date < DateOnly.FromDateTime(DateTime.Today))
            throw new DomainException("Cannot create forecast for past dates");

        return new WeatherForecast
        {
            Id = Guid.NewGuid(),
            Date = date,
            Temperature = temperature,
            Condition = condition,
            City = city
        };
    }

    public bool IsHot() => Temperature.Celsius > 30;
    public bool IsCold() => Temperature.Celsius < 10;
    public bool IsFreezing() => Temperature.Celsius < 0;
}

public enum WeatherCondition
{
    Sunny,
    Cloudy,
    Rainy,
    Snowy,
    Stormy
}
```

---

## Recursos Adicionales

### Libros Recomendados
- **"Domain-Driven Design"** - Eric Evans (Blue Book)
- **"Implementing Domain-Driven Design"** - Vaughn Vernon (Red Book)
- **"Domain-Driven Design Distilled"** - Vaughn Vernon

### Patrones Relacionados
- **CQRS** (Command Query Responsibility Segregation)
- **Event Sourcing**
- **Saga Pattern**

---

## Conclusión

**DDD + Arquitectura Hexagonal** es una combinación poderosa:

- ✅ **DDD** te da las herramientas para modelar el dominio
- ✅ **Hexagonal** te da la estructura para aislar el dominio
- ✅ Juntos crean aplicaciones mantenibles y escalables

**Próximos pasos:**
1. Identificar los conceptos clave de tu dominio
2. Crear Value Objects para conceptos importantes
3. Modelar Aggregates con sus invariantes
4. Implementar Domain Events para comunicación
5. Usar Specifications para reglas de negocio reutilizables
