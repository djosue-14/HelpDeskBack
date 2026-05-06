using System.Reflection;
using FluentValidation;
using HelpDesk.Application.Interfaces;
using HelpDesk.Application.Mappings;
using HelpDesk.Application.Services;
using HelpDesk.Domain.Interfaces;
using HelpDesk.Infrastructure.Persistence.Contexts;
using HelpDesk.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// OpenAPI nativo (JSON en /openapi/v1.json)
builder.Services.AddOpenApi();

// Swagger / Swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "HelpDesk API",
        Version = "v1",
        Description = """
            API REST para el sistema de gestión de tickets HelpDesk.

            Permite administrar el ciclo de vida completo de los tickets de soporte,
            incluyendo creación, asignación de agentes, seguimiento de SLA,
            comentarios, adjuntos, métricas de rendimiento y reputación de agentes.
            """,
        Contact = new OpenApiContact
        {
            Name = "Equipo HelpDesk",
            Email = "helpdesk@company.com"
        },
        License = new OpenApiLicense
        {
            Name = "MIT",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    // XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);

    // JWT Bearer security definition
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Introduce el token JWT en el formato: Bearer {token}"
    });

    // Apply JWT requirement to all endpoints
    options.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer"),
            new List<string>()
        }
    });
});

// Database
builder.Services.AddDbContext<HelpDeskDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()));

// AutoMapper
builder.Services.AddAutoMapper(cfg => cfg.AddMaps(typeof(MappingProfile).Assembly));

// FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(MappingProfile).Assembly);

// Application services
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<ITicketCommentService, TicketCommentService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<ISupportTypeService, SupportTypeService>();
builder.Services.AddScoped<ISupportTypeAgentService, SupportTypeAgentService>();
builder.Services.AddScoped<ISlaConfigurationService, SlaConfigurationService>();
builder.Services.AddScoped<IScoreService, ScoreService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

// Unit of work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// CORS
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

// Health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "HelpDesk API v1");
        options.RoutePrefix = "swagger";
        options.DocumentTitle = "HelpDesk API - Swagger UI";
        options.DisplayRequestDuration();
        options.EnableTryItOutByDefault();
        options.EnableFilter();
    });
}

app.UseHttpsRedirection();
app.UseCors();

// TODO: SOLO DESARROLLO — eliminar este bloque al activar autenticación real (JWT/OAuth)
if (app.Environment.IsDevelopment())
{
    app.Use(async (ctx, next) =>
    {
        var identity = new System.Security.Claims.ClaimsIdentity(
        [
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "dev-user"),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "Administrator")
        ], "DevBypass");
        ctx.User = new System.Security.Claims.ClaimsPrincipal(identity);
        await next();
    });
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
