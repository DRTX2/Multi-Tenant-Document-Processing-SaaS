using AspNetProject.Application.Services;
using AspNetProject.Domain.Ports.In;
using AspNetProject.Domain.Ports.Out;
using AspNetProject.Infrastructure.Persistence;
using AspNetProject.Infrastructure.Persistence.Repositories;
using AspNetProject.Infrastructure.Providers;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using AspNetProject.Application.Validators.Tenants;
using AspNetProject.Application.Validators.Users;
using AspNetProject.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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

// ----- Authentication & Security Configuration -----

// 1. Bind JwtSettings
var jwtSection = builder.Configuration.GetSection(JwtSettings.SectionName);
builder.Services.Configure<JwtSettings>(jwtSection);
// cambiarlo luego.
var jwtSettings = jwtSection.Get<JwtSettings>() ?? new JwtSettings 
{ 
    Secret = "SuperSecretKeyForDevelopmentOnly12345!", 
    Issuer= "SecureDocs", 
    Audience="SecureDocsUsers", 
    ExpiryMinutes=60 
};

// 2. Register Auth Services
builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
builder.Services.AddSingleton<ITokenGenerator, JwtTokenGenerator>();

// 3. Add Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret))
    };
});

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

app.UseAuthentication(); // Must be before Authorization
app.UseAuthorization();

app.MapControllers();

app.Run();

