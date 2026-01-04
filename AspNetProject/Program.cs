using AspNetProject.Application.Services;
using AspNetProject.Domain.Ports.In;
using AspNetProject.Domain.Ports.Out;
using AspNetProject.Infrastructure.Persistence;
using AspNetProject.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using AspNetProject.Application.Validators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddValidatorsFromAssemblyContaining<CreateTenantValidator>();

// Configurar Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "SecureDocs Cloud API",
        Version = "v1",
        Description = "Enterprise Multi-Tenant Document SaaS"
    });
});

// Configuración de Entity Framework Core con PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Host=localhost;Database=securedocs;Username=postgres;Password=postgres";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorCodesToAdd: null);
        npgsqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
    });
    
    // Habilitar logging sensible solo en desarrollo
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// Registrar UnitOfWork
builder.Services.AddScoped<IUnitOfWork, EfUnitOfWork>();

// Registrar Repositorios Generic y Específicos
builder.Services.AddScoped(typeof(IRepository<,>), typeof(EfRepository<,>));
builder.Services.AddScoped<ITenantRepository, TenantRepository>();
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuditRepository, AuditRepository>();
builder.Services.AddScoped<IDocumentJobRepository, DocumentJobRepository>();

// Registrar Servicios de Aplicación (Ports In)
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IDocumentProcessingService, DocumentProcessingService>();

// Mocks placeholders for missing Infrastructure Components
// In real app, these would be concrete implementations (S3, RabbitMQ)
builder.Services.AddScoped<IFileStorage, MockFileStorage>(); 
builder.Services.AddScoped<IDomainEventDispatcher, MockEventDispatcher>();

// Registrar Workers (Background Services)
builder.Services.AddHostedService<AspNetProject.Infrastructure.BackgroundJobs.DocumentProcessingWorker>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "SecureDocs API v1");
    });
}
else 
{
    // Production settings
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

// --- Mocks Definitions (Can be moved to Infrastructure/Providers later) ---
public class MockFileStorage : IFileStorage
{
    public Task<string> UploadAsync(string fileName, Stream content, CancellationToken cancellationToken = default)
    {
        // Simulate upload
        return Task.FromResult($"local-storage/{fileName}");
    }

    public Task<Stream> DownloadAsync(string fileName, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Stream.Null);
    }
}

public class MockEventDispatcher : IDomainEventDispatcher
{
    private readonly ILogger<MockEventDispatcher> _logger;
    public MockEventDispatcher(ILogger<MockEventDispatcher> logger) => _logger = logger;

    public Task DispatchAsync(AspNetProject.Domain.Events.IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("PUBLISHING EVENT: {EventType} occurred at {Date}", domainEvent.GetType().Name, domainEvent.OccurredOn);
        return Task.CompletedTask;
    }
}