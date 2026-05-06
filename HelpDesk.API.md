# HelpDesk.API

## 📋 Descripción General

**HelpDesk.API** es la capa de presentación del proyecto HelpDesk, exponiendo una RESTful API construida con ASP.NET Core 10.0. Este proyecto actúa como punto de entrada de la aplicación, coordinando las peticiones HTTP con la capa de aplicación y devolviendo respuestas apropiadas a los clientes del sistema de gestión de tickets.

---

## 🎯 Propósito

Este proyecto proporciona:
- **API RESTful**: Endpoints HTTP para todas las operaciones del ciclo de vida del ticket
- **Punto de entrada**: Composición root y configuración de la aplicación
- **Controladores**: Manejo de peticiones y respuestas HTTP por recurso
- **Inyección de dependencias**: Configuración del contenedor IoC
- **Middleware pipeline**: CORS, autenticación, autorización
- **Documentación API**: OpenAPI/Swagger

---

## 🏗️ Arquitectura

### Patrón Arquitectónico
- **Clean Architecture** — Capa de presentación (externa)
- **RESTful API**: Diseño basado en recursos del dominio HelpDesk
- **MVC Pattern**: Model-View-Controller (sin Views)
- **Dependency Injection**: ASP.NET Core IoC Container

### Principios Aplicados
- ✅ **API-First**: Diseño centrado en la API
- ✅ **HTTP Semantics**: Uso correcto de verbos y códigos de estado
- ✅ **Stateless**: Sin estado en el servidor
- ✅ **Resource-Based**: URLs representan recursos del dominio

---

## 📦 Dependencias

### Proyectos Referenciados
```
HelpDesk.API
    ├── HelpDesk.Application (Servicios y DTOs)
    └── HelpDesk.Infrastructure (DbContext y repositorios)
```

### Paquetes NuGet

| Paquete | Versión | Propósito |
|---------|---------|-----------|
| `Microsoft.AspNetCore.OpenApi` | 10.0.0 | Generación de especificación OpenAPI nativa |
| `Swashbuckle.AspNetCore` | 10.1.7 | Swagger UI + SwaggerGen |
| `Microsoft.OpenApi` | 2.4.1 | Modelos OpenAPI para la configuración de Swagger |
| `Microsoft.EntityFrameworkCore` | 10.0.0 | Para migraciones desde la API |
| `Microsoft.EntityFrameworkCore.Design` | 10.0.0 | Herramientas de diseño EF |
| `Microsoft.EntityFrameworkCore.SqlServer` | 10.0.0 | Provider de SQL Server |
| `Microsoft.EntityFrameworkCore.Tools` | 10.0.0 | Comandos dotnet ef |

### Framework
- **ASP.NET Core 10.0** (Web SDK)
- **Nullable Reference Types**: Habilitado
- **Implicit Usings**: Habilitado

---

## 📁 Estructura del Proyecto

```
HelpDesk.API/
├── Controllers/
│   ├── Base/
│   │   ├── BaseApiController.cs              # Clase base centraliza [ApiController], [Route], [Produces], helpers
│   │   └── README.md                         # Documentación del patrón base
│   ├── TicketsController.cs                  # Ciclo de vida completo del ticket
│   ├── TicketCommentsController.cs           # Comentarios e hilo (ruta custom: /api/Tickets/{id}/comments)
│   ├── DepartmentsController.cs              # Gestión de departamentos
│   ├── SupportTypesController.cs             # Gestión de tipos de soporte
│   ├── SupportTypeAgentsController.cs        # Asignación de agentes a tipos de soporte
│   ├── SlaConfigurationsController.cs        # Configuración de tiempos SLA
│   ├── ScoreController.cs                    # Consulta de reputación del usuario
│   └── DashboardController.cs                # Métricas y vistas por rol de usuario
│
├── Program.cs                                # Punto de entrada y configuración
├── appsettings.json                          # Configuración general
├── appsettings.Development.json              # Configuración de desarrollo
└── Properties/
    └── launchSettings.json                   # Configuración de ejecución
```

---

## 🔑 Componentes Clave

### 1. Program.cs — Composición Root

#### **Configuración de Servicios**
```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// OpenAPI nativo + Swagger UI
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "HelpDesk API", Version = "v1" });
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFile));
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization", Type = SecuritySchemeType.Http,
        Scheme = "bearer", BearerFormat = "JWT", In = ParameterLocation.Header
    });
    options.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
    {
        { new OpenApiSecuritySchemeReference("Bearer"), new List<string>() }
    });
});

// Database Configuration
builder.Services.AddDbContext<HelpDeskDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()));

// AutoMapper — DI incluido desde v16; usar AddMaps en lugar de AddAutoMapper(Assembly)
builder.Services.AddAutoMapper(cfg => cfg.AddMaps(typeof(MappingProfile).Assembly));

// FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(MappingProfile).Assembly);

// Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Application Services
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<ITicketCommentService, TicketCommentService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<ISupportTypeService, SupportTypeService>();
builder.Services.AddScoped<ISupportTypeAgentService, SupportTypeAgentService>();
builder.Services.AddScoped<ISlaConfigurationService, SlaConfigurationService>();
builder.Services.AddScoped<IScoreService, ScoreService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

// CORS
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddHealthChecks();
```

#### **Configuración de Middleware**
```csharp
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "HelpDesk API v1");
        options.RoutePrefix = "swagger";
        options.EnableTryItOutByDefault();
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
```

---

### 2. BaseApiController

Clase base abstracta que centraliza el boilerplate repetitivo. Todos los controladores la heredan.

```csharp
// HelpDesk.API/Controllers/Base/BaseApiController.cs
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class BaseApiController : ControllerBase
{
    // Extrae el NameIdentifier del ClaimsPrincipal (token JWT en prod, DevBypass en dev)
    protected string GetCurrentUser() =>
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

    protected string GetCurrentRole() =>
        User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

    // Result<T> exitoso → 200 OK | fallido → 400 Bad Request
    protected IActionResult HandleResult<T>(Result<T> result) =>
        result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });

    // Result exitoso → 200 OK | fallido → 400 Bad Request
    protected IActionResult HandleResult(Result result) =>
        result.IsSuccess ? Ok() : BadRequest(new { error = result.Error });

    // Para GET por ID: fallido → 404 Not Found
    protected IActionResult HandleGetResult<T>(Result<T> result) =>
        result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });

    // Para POST: exitoso → 201 Created con Location header
    protected IActionResult HandleCreateResult<T>(Result<T> result, string actionName, object routeValues) =>
        result.IsSuccess
            ? CreatedAtAction(actionName, routeValues, result.Value)
            : BadRequest(new { error = result.Error });

    // Para DELETE: exitoso → 204 No Content
    protected IActionResult HandleDeleteResult(Result result) =>
        result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
}
```

**Beneficios:**
- Los 8 controladores no llevan `[ApiController]`, `[Route]` ni `[Produces]` — heredan del base
- `TicketCommentsController` conserva su ruta custom `[Route("api/Tickets/{ticketId:int}/comments")]` que sobreescribe la del base
- `GetCurrentUser()` y `GetCurrentRole()` extraen claims del `HttpContext.User` — funciona igual con JWT real o con el middleware de bypass de desarrollo

---

### 3. Controladores

#### **SupportTypeAgentsController**
Expone las operaciones de asignación y consulta de agentes por tipo de soporte. El `userId` del agente a asignar se extrae del `HttpContext.User` en el controlador y se pasa como parámetro al servicio — nunca se recibe en el body del request para evitar suplantación.

```csharp
public class SupportTypeAgentsController : BaseApiController
{
    private readonly ISupportTypeAgentService _supportTypeAgentService;

    public SupportTypeAgentsController(ISupportTypeAgentService supportTypeAgentService)
    {
        _supportTypeAgentService = supportTypeAgentService;
    }

    /// <summary>Retorna la asignación activa del tipo de soporte indicado.</summary>
    [HttpGet("active/{supportTypeId:int}")]
    public async Task<IActionResult> GetActive(int supportTypeId, CancellationToken cancellationToken)
    {
        var result = await _supportTypeAgentService.GetActiveAgentAsync(supportTypeId, cancellationToken);
        return HandleGetResult(result);
    }

    /// <summary>Retorna el historial completo de asignaciones (activas e inactivas).</summary>
    [HttpGet("history/{supportTypeId:int}")]
    public async Task<IActionResult> GetHistory(int supportTypeId, CancellationToken cancellationToken)
    {
        var result = await _supportTypeAgentService.GetHistoryAsync(supportTypeId, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Asigna un agente al tipo de soporte indicado.
    /// El UserId se toma del ClaimsPrincipal; el body solo lleva el SupportTypeId.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Assign(
        [FromBody] AssignAgentRequest request,
        CancellationToken cancellationToken)
    {
        var currentUser = GetCurrentUser();
        request.UserId = currentUser;  // UserId extraído del token — nunca del body
        var result = await _supportTypeAgentService.AssignAsync(request, currentUser, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>Desactiva la asignación activa del tipo de soporte (soft delete).</summary>
    [HttpDelete("{supportTypeId:int}")]
    public async Task<IActionResult> Unassign(int supportTypeId, CancellationToken cancellationToken)
    {
        var result = await _supportTypeAgentService.UnassignAsync(supportTypeId, GetCurrentUser(), cancellationToken);
        return HandleDeleteResult(result);
    }
}
```

#### **DashboardController**
Expone las vistas por rol y las métricas operativas. El `userId` siempre se extrae mediante `GetCurrentUser()` y el `role` mediante `GetCurrentRole()`.

```csharp
public class DashboardController : BaseApiController
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>Vista general del dashboard según el rol del usuario autenticado.</summary>
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUser();
        var role   = GetCurrentRole();

        return role switch
        {
            "Requester"     => HandleResult(await _dashboardService.GetRequesterDashboardAsync(userId, cancellationToken)),
            "SupportAgent"  => HandleResult(await _dashboardService.GetAgentDashboardAsync(userId, cancellationToken)),
            "Coordinator"   => HandleResult(await _dashboardService.GetCoordinatorDashboardAsync(userId, cancellationToken)),
            "Administrator" => HandleResult(await _dashboardService.GetAdminDashboardAsync(cancellationToken)),
            _               => Forbid()
        };
    }

    /// <summary>KPIs operativos del período seleccionado (US-22).</summary>
    [HttpGet("metrics")]
    public async Task<IActionResult> GetMetrics(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] int? departmentId,
        CancellationToken cancellationToken)
    {
        var result = await _dashboardService.GetOperationalMetricsAsync(from, to, departmentId, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>Rendimiento individual del agente (US-23).</summary>
    [HttpGet("agents/{agentId}/performance")]
    public async Task<IActionResult> GetAgentPerformance(
        string agentId,
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        CancellationToken cancellationToken)
    {
        var result = await _dashboardService.GetAgentPerformanceAsync(agentId, from, to, cancellationToken);
        return HandleGetResult(result);
    }

    /// <summary>Tabla comparativa del equipo por departamento (US-23).</summary>
    [HttpGet("departments/{departmentId:int}/team-performance")]
    public async Task<IActionResult> GetTeamPerformance(
        int departmentId,
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        CancellationToken cancellationToken)
    {
        var result = await _dashboardService.GetTeamPerformanceAsync(departmentId, from, to, cancellationToken);
        return HandleGetResult(result);
    }

    /// <summary>Mapa de calor departamento × tipo de soporte (US-24).</summary>
    [HttpGet("heatmap")]
    public async Task<IActionResult> GetHeatMap(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] IEnumerable<int>? departmentIds,
        CancellationToken cancellationToken)
    {
        var result = await _dashboardService.GetHeatMapAsync(from, to, departmentIds, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>Ranking mensual de reputación — Top 10 (US-18).</summary>
    [HttpGet("leaderboard")]
    public async Task<IActionResult> GetLeaderboard(
        [FromQuery] int year,
        [FromQuery] int month,
        CancellationToken cancellationToken)
    {
        var result = await _dashboardService.GetMonthlyLeaderboardAsync(year, month, cancellationToken);
        return HandleResult(result);
    }
}
```

#### **TicketsController**
```csharp
public class TicketsController : BaseApiController
{
    private readonly ITicketService _ticketService;

    public TicketsController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _ticketService.GetAllAsync(cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _ticketService.GetByIdAsync(id, cancellationToken);
        return HandleGetResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateTicketRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _ticketService.CreateAsync(request, GetCurrentUser(), cancellationToken);
        return HandleCreateResult(result, nameof(GetById), new { id = result.Value?.TicketId });
    }

    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> ChangeStatus(
        int id,
        [FromBody] UpdateTicketStatusRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _ticketService.ChangeStatusAsync(id, request, GetCurrentUser(), cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("{id:int}/close")]
    public async Task<IActionResult> Close(
        int id,
        [FromBody] CloseTicketRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _ticketService.CloseAsync(id, request, GetCurrentUser(), cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("{id:int}/reopen")]
    public async Task<IActionResult> Reopen(
        int id,
        [FromBody] ReopenTicketRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _ticketService.ReopenAsync(id, request.Reason, GetCurrentUser(), cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("{id:int}/redirect")]
    public async Task<IActionResult> Redirect(
        int id,
        [FromBody] RedirectTicketRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _ticketService.RedirectAsync(id, request.NewSupportTypeId, GetCurrentUser(), cancellationToken);
        return HandleResult(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _ticketService.DeleteAsync(id, GetCurrentUser(), cancellationToken);
        return HandleDeleteResult(result);
    }
}
```

---

## 🌐 API Endpoints

### Tickets

| Método | Endpoint | Descripción | Response |
|--------|----------|-------------|----------|
| `GET` | `/api/Tickets` | Obtener todos los tickets | `200 OK` + Array de TicketSummaryDto |
| `GET` | `/api/Tickets/{id}` | Obtener ticket completo por ID | `200 OK` + TicketDto / `404 Not Found` |
| `POST` | `/api/Tickets` | Crear nuevo ticket | `201 Created` + TicketDto / `400 Bad Request` |
| `PUT` | `/api/Tickets/{id}/status` | Cambiar estado del ticket | `200 OK` + TicketDto / `400 Bad Request` |
| `PUT` | `/api/Tickets/{id}/close` | Cerrar ticket con categoría | `200 OK` + TicketDto / `400 Bad Request` |
| `PUT` | `/api/Tickets/{id}/reopen` | Reabrir ticket (dentro de gracia) | `200 OK` + TicketDto / `400 Bad Request` |
| `PUT` | `/api/Tickets/{id}/redirect` | Redirigir a otro tipo de soporte | `200 OK` + TicketDto / `400 Bad Request` |
| `DELETE` | `/api/Tickets/{id}` | Soft delete del ticket | `200 OK` / `404 Not Found` |

### Comentarios del Ticket

| Método | Endpoint | Descripción | Response |
|--------|----------|-------------|----------|
| `GET` | `/api/Tickets/{id}/comments` | Obtener hilo de comentarios | `200 OK` + Array de TicketCommentDto |
| `POST` | `/api/Tickets/{id}/comments` | Agregar comentario o nota interna | `201 Created` + TicketCommentDto / `400 Bad Request` |

### Departamentos

| Método | Endpoint | Descripción | Response |
|--------|----------|-------------|----------|
| `GET` | `/api/Departments` | Obtener todos los departamentos | `200 OK` + Array de DepartmentDto |
| `GET` | `/api/Departments/{id}` | Obtener departamento por ID | `200 OK` + DepartmentDto / `404 Not Found` |
| `POST` | `/api/Departments` | Crear departamento | `201 Created` + DepartmentDto / `400 Bad Request` |
| `DELETE` | `/api/Departments/{id}` | Eliminar departamento | `200 OK` / `404 Not Found` |

### Tipos de Soporte

| Método | Endpoint | Descripción | Response |
|--------|----------|-------------|----------|
| `GET` | `/api/SupportTypes` | Obtener todos los tipos de soporte | `200 OK` + Array de SupportTypeDto |
| `GET` | `/api/SupportTypes/{id}` | Obtener tipo de soporte por ID | `200 OK` + SupportTypeDto / `404 Not Found` |
| `GET` | `/api/Departments/{id}/SupportTypes` | Tipos de soporte por departamento | `200 OK` + Array de SupportTypeDto |
| `POST` | `/api/SupportTypes` | Crear tipo de soporte | `201 Created` + SupportTypeDto / `400 Bad Request` |
| `DELETE` | `/api/SupportTypes/{id}` | Eliminar tipo de soporte | `200 OK` / `404 Not Found` |

### Asignación de Agentes (SupportTypeAgents)

> El `UserId` del agente **siempre** se extrae del token JWT en el servidor. El body del `POST` únicamente lleva el `SupportTypeId`.

| Método | Endpoint | Descripción | Response |
|--------|----------|-------------|----------|
| `GET` | `/api/SupportTypeAgents/active/{supportTypeId}` | Asignación activa del tipo de soporte | `200 OK` + SupportTypeAgentDto / `404 Not Found` |
| `GET` | `/api/SupportTypeAgents/history/{supportTypeId}` | Historial completo de asignaciones | `200 OK` + Array de SupportTypeAgentDto |
| `POST` | `/api/SupportTypeAgents` | Asignar agente (UserId del token) | `200 OK` + SupportTypeAgentDto / `400 Bad Request` |
| `DELETE` | `/api/SupportTypeAgents/{supportTypeId}` | Desasignar agente activo (soft delete) | `200 OK` / `404 Not Found` |

### Dashboard y Métricas

| Método | Endpoint | Descripción | Response |
|--------|----------|-------------|----------|
| `GET` | `/api/Dashboard` | Vista general según rol del token | `200 OK` + DTO según rol / `400 Bad Request` |
| `GET` | `/api/Dashboard/metrics` | KPIs operativos del período (US-22) | `200 OK` + OperationalMetricsDto |
| `GET` | `/api/Dashboard/agents/{agentUserId}/performance` | Rendimiento individual del agente (US-23) | `200 OK` + AgentPerformanceDto |
| `GET` | `/api/Dashboard/departments/{departmentId}/team-performance` | Comparativa del equipo (US-23) | `200 OK` + Array de AgentPerformanceDto |
| `GET` | `/api/Dashboard/heatmap` | Mapa de calor depto × tipo soporte (US-24) | `200 OK` + HeatMapDto |
| `GET` | `/api/Dashboard/leaderboard` | Ranking mensual de reputación (US-18) | `200 OK` + LeaderboardDto |

### Configuración SLA

| Método | Endpoint | Descripción | Response |
|--------|----------|-------------|----------|
| `GET` | `/api/SlaConfigurations` | Obtener configuraciones de SLA | `200 OK` + Array de SlaConfigurationDto |
| `PUT` | `/api/SlaConfigurations/{priority}` | Actualizar horas límite de prioridad | `200 OK` + SlaConfigurationDto / `400 Bad Request` |

### Reputación

| Método | Endpoint | Descripción | Response |
|--------|----------|-------------|----------|
| `GET` | `/api/Score/{username}` | Obtener perfil de reputación | `200 OK` + UserScoreDto / `404 Not Found` |
| `POST` | `/api/Tickets/{id}/rate` | Calificar ticket cerrado | `200 OK` / `400 Bad Request` |

### Health Check

| Método | Endpoint | Descripción | Response |
|--------|----------|-------------|----------|
| `GET` | `/health` | Estado de la aplicación | `200 OK` + Status |

---

## 📡 Ejemplos de Request/Response

### POST /api/Tickets
**Request Body:**
```json
{
  "departmentId": 1,
  "supportTypeId": 3,
  "priority": "High",
  "subject": "Error al acceder al sistema de nómina",
  "description": "Desde esta mañana no puedo iniciar sesión en el sistema de nómina. El error es: 'Invalid credentials'."
}
```

**Response 201 Created:**
```json
{
  "ticketId": 1042,
  "ticketNumber": 1042,
  "subject": "Error al acceder al sistema de nómina",
  "description": "Desde esta mañana no puedo iniciar sesión...",
  "departmentName": "IT",
  "supportTypeName": "Accesos y Credenciales",
  "priority": "High",
  "status": "Open",
  "resolutionCategory": null,
  "assignedUserId": "jlopez",
  "createdAt": "2026-04-24T09:00:00Z",
  "createdBy": "mgarcia",
  "firstOpenedAt": null,
  "workStartedAt": null,
  "deadline": "2026-04-24T13:00:00Z",
  "closedAt": null,
  "totalPausedMinutes": 0,
  "comments": [],
  "attachments": []
}
```

**Response 400 Bad Request:**
```json
{
  "error": "Priority must be a valid value: Critical, High, Medium, Low"
}
```

### PUT /api/Tickets/{id}/close
**Request Body:**
```json
{
  "resolutionCategory": "Resolved",
  "closingComment": "Se restableció la contraseña del usuario y se verificó el acceso correctamente."
}
```

**Response 200 OK:**
```json
{
  "ticketId": 1042,
  "ticketNumber": 1042,
  "status": "Closed",
  "resolutionCategory": "Resolved",
  "closedAt": "2026-04-24T11:45:00Z"
}
```

### POST /api/SupportTypeAgents — Asignar agente activo
**Request Body** (solo SupportTypeId — el UserId viene del token):
```json
{
  "supportTypeId": 3
}
```

**Response 200 OK:**
```json
{
  "supportTypeAgentId": 7,
  "supportTypeId": 3,
  "supportTypeName": "Accesos y Credenciales",
  "userId": "jlopez",
  "isEnabled": true,
  "createdAt": "2026-04-24T09:00:00Z",
  "createdBy": "jlopez",
  "disabledAt": null,
  "disabledBy": null
}
```

### GET /api/Dashboard/metrics?from=2026-04-01&to=2026-04-24
**Response 200 OK:**
```json
{
  "from": "2026-04-01T00:00:00Z",
  "to": "2026-04-24T00:00:00Z",
  "totalCreated": 142,
  "totalClosed": 118,
  "totalActive": 24,
  "slaCompliancePct": 91.5,
  "avgResolutionHours": 6.3,
  "avgFirstResponseHours": 1.2,
  "reopenRatePct": 3.4,
  "redirectRatePct": 8.1,
  "dailyTrend": [
    { "date": "2026-04-01", "created": 7, "closed": 5 },
    { "date": "2026-04-02", "created": 4, "closed": 6 }
  ],
  "slaByPriority": {
    "Critical": 100.0,
    "High": 94.2,
    "Medium": 90.1,
    "Low": 85.7
  }
}
```

### GET /api/Dashboard/heatmap?from=2026-04-01&to=2026-04-24
**Response 200 OK:**
```json
{
  "from": "2026-04-01T00:00:00Z",
  "to": "2026-04-24T00:00:00Z",
  "rows": [
    {
      "departmentId": "...",
      "departmentName": "IT",
      "cells": [
        {
          "supportTypeId": "...",
          "supportTypeName": "Hardware",
          "volume": 38,
          "slaBreachPct": 7.9
        },
        {
          "supportTypeId": "...",
          "supportTypeName": "Accesos y Credenciales",
          "volume": 21,
          "slaBreachPct": 2.1
        }
      ]
    }
  ]
}
```

### PUT /api/SlaConfigurations/High
**Request Body:**
```json
{
  "hoursLimit": 6
}
```

**Response 200 OK:**
```json
{
  "priority": "High",
  "hoursLimit": 6,
  "lastModifiedAt": "2026-04-24T10:00:00Z",
  "lastModifiedBy": "admin"
}
```

### GET /health
**Response 200 OK:**
```json
{
  "status": "Healthy",
  "timestamp": "2026-04-24T10:00:00Z"
}
```

---

## 🎨 Patrones Utilizados

### 1. **Controller Pattern**
- Controladores delgados, sin lógica de negocio
- Delegación completa a los servicios de aplicación
- Manejo del Result Pattern para mapear a respuestas HTTP

### 2. **Dependency Injection**
- Constructor injection en todos los controladores
- Scoped lifetimes para servicios y Unit of Work
- Configuración centralizada en Program.cs

### 3. **RESTful Design**
- Recursos como sustantivos (`Tickets`, `Departments`)
- Verbos HTTP semánticos para acciones del ciclo de vida
- Sub-recursos para acciones contextuales (`/tickets/{id}/close`)

### 4. **BaseApiController — Result to HTTP Mapping**
```csharp
// Centralizado en BaseApiController — los controladores llaman al helper apropiado:
return HandleResult(result);         // 200 OK / 400 Bad Request
return HandleGetResult(result);      // 200 OK / 404 Not Found
return HandleCreateResult(result, nameof(GetById), new { id = result.Value?.TicketId });  // 201 Created
return HandleDeleteResult(result);   // 204 No Content / 400 Bad Request
```

---

## 🔧 Configuración

### appsettings.Development.json
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=HelpDesk;User Id=SA;Password=Developer.14;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

### CORS Policy (Default Policy)
```csharp
// Program.cs — política por defecto (AllowAnyOrigin en desarrollo)
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

// Middleware
app.UseCors();
```

**⚠️ Nota:** En producción, configurar CORS con orígenes específicos.

### Autenticación en Desarrollo (Dev Bypass)

Durante el desarrollo, se usa un middleware que inyecta un `ClaimsPrincipal` falso antes de `UseAuthentication()`. Esto permite probar todos los endpoints sin un token JWT real:

```csharp
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
```

Para cambiar el rol o usuario durante el desarrollo, editar los valores `"dev-user"` y `"Administrator"`. Para activar autenticación real, eliminar este bloque — los controladores ya leen los claims a través de `GetCurrentUser()` y `GetCurrentRole()` en `BaseApiController`.

---

## 📖 Documentación Swagger

### Acceso

| Entorno | URL | Descripción |
|---------|-----|-------------|
| Desarrollo | `http://localhost:5000/swagger` | Swagger UI interactivo |
| Desarrollo | `http://localhost:5000/swagger/v1/swagger.json` | Especificación OpenAPI 3.0 (JSON) |
| Desarrollo | `http://localhost:5000/openapi/v1.json` | Especificación nativa ASP.NET Core |
| Producción | *(no expuesto)* | Swagger deshabilitado en producción |

### Características de la UI

- **Try it out**: Todos los endpoints habilitados por defecto para pruebas directas desde el navegador
- **JWT Bearer Auth**: Botón "Authorize" para introducir el token JWT; se propaga a todas las requests
- **Duración de requests**: Muestra el tiempo de respuesta de cada llamada
- **Filtro**: Campo de búsqueda para filtrar endpoints por nombre o tag
- **Tags agrupados**: Endpoints agrupados por controlador (Tickets, Departments, SupportTypes, etc.)

### Configuración en Program.cs

```csharp
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "HelpDesk API",
        Version = "v1",
        Description = "API REST para el sistema de gestión de tickets HelpDesk.",
        Contact = new OpenApiContact { Name = "Equipo HelpDesk", Email = "helpdesk@company.com" },
        License = new OpenApiLicense { Name = "MIT" }
    });

    // Incluye los comentarios XML de los controladores
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFile));

    // JWT Bearer — define el esquema de seguridad
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Introduce el token JWT en el formato: Bearer {token}"
    });

    // Aplica el requisito JWT a todos los endpoints
    options.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
    {
        { new OpenApiSecuritySchemeReference("Bearer"), new List<string>() }
    });
});

// Middleware (solo en Development)
if (app.Environment.IsDevelopment())
{
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
```

### Generación de XML Docs

El `.csproj` tiene habilitada la generación del archivo XML con los comentarios de documentación:

```xml
<PropertyGroup>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <NoWarn>$(NoWarn);1591</NoWarn>  <!-- Suprime CS1591 en miembros sin comentar -->
</PropertyGroup>
```

El XML se genera en `bin/Debug/net10.0/HelpDesk.API.xml` y Swagger lo incluye automáticamente en tiempo de ejecución.

### Comentarios XML en controladores

Cada endpoint documenta:
- **`<summary>`** — Descripción corta que aparece como título del endpoint
- **`<remarks>`** — Descripción extendida con reglas de negocio, valores válidos de enums, etc.
- **`<param>`** — Documentación de cada parámetro (ruta, query, body)
- **`<response code="200">`** — Respuesta exitosa con tipo del DTO devuelto
- **`[ProducesResponseType]`** — Tipos de respuesta que Swagger usa para los schemas

Ejemplo:
```csharp
/// <summary>Crea un nuevo ticket de soporte.</summary>
/// <remarks>
/// El sistema asigna automáticamente:
/// - **Deadline** según la configuración SLA de la prioridad indicada
/// - **AssignedAgent** según el agente activo del tipo de soporte
/// </remarks>
/// <param name="request">Datos del ticket a crear.</param>
/// <response code="201">Ticket creado. Devuelve el ticket con todos sus campos calculados.</response>
/// <response code="400">Error de validación o referencia inválida.</response>
[HttpPost]
[ProducesResponseType(typeof(TicketDto), StatusCodes.Status201Created)]
[ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
public async Task<IActionResult> Create([FromBody] CreateTicketRequest request, CancellationToken ct)
```

### Paquetes utilizados

| Paquete | Versión | Propósito |
|---------|---------|-----------|
| `Swashbuckle.AspNetCore` | 10.1.7 | Swagger UI + generación del JSON |
| `Microsoft.AspNetCore.OpenApi` | 10.0.0 | Endpoint nativo `/openapi/v1.json` |
| `Microsoft.OpenApi` | 2.4.1 | Modelos OpenAPI 2.x (`OpenApiInfo`, `OpenApiSecurityScheme`, etc.) |

> **Nota sobre Microsoft.OpenApi 2.x:** En la versión 2.x todos los tipos están en el namespace raíz `Microsoft.OpenApi` (no en `Microsoft.OpenApi.Models` como en 1.x). Además, `OpenApiSecurityRequirement` usa `OpenApiSecuritySchemeReference` como clave y acepta `Func<OpenApiDocument, OpenApiSecurityRequirement>` en `AddSecurityRequirement`.

### Endpoints documentados

| Controlador | Tag Swagger | Endpoints |
|-------------|-------------|-----------|
| `TicketsController` | Tickets | 8 endpoints: GET all, GET {id}, POST, PUT status/close/reopen/redirect, DELETE |
| `TicketCommentsController` | TicketComments | 2 endpoints: GET comments, POST add comment |
| `DepartmentsController` | Departments | 5 endpoints: GET all, GET {id}, GET {id}/supporttypes, POST, DELETE |
| `SupportTypesController` | SupportTypes | 4 endpoints: GET all, GET {id}, POST, DELETE |
| `SupportTypeAgentsController` | SupportTypeAgents | 4 endpoints: GET active, GET history, POST assign, DELETE unassign |
| `SlaConfigurationsController` | SlaConfigurations | 2 endpoints: GET all, PUT {priority} |
| `ScoreController` | Score | 2 endpoints: GET {userId}, POST /api/Tickets/{id}/rate |
| `DashboardController` | Dashboard | 6 endpoints: GET (role-based), metrics, agent performance, team, heatmap, leaderboard |

---

## ✨ Características Destacadas

### OpenAPI/Swagger
- **Generación automática** de documentación
- **Interfaz interactiva** para probar todos los endpoints
- **Disponible** solo en entorno de desarrollo

### Health Check
- **Endpoint simple** para monitoreo y orquestación
- **Timestamp UTC** para tracking de uptime
- **Extensible** para agregar checks de BD y dependencias externas

### CancellationToken
- **Soporte** en todos los endpoints async
- **Cancelación cooperativa** de operaciones largas
- **Mejora** la responsividad ante desconexiones del cliente

### Result Pattern Integration
```csharp
var result = await _ticketService.CloseAsync(id, request, currentUser, cancellationToken);

if (result.IsFailure)
    return BadRequest(new { error = result.Error });

return Ok(result.Value);
```

---

## 🔗 Dependencias de Este Proyecto

- **HelpDesk.Application** ← Usa servicios e interfaces
- **HelpDesk.Infrastructure** ← Configura DbContext y UnitOfWork

---

## 📝 Convenciones

### Routing
- **Base route**: `/api/[controller]` — definido en `BaseApiController`, heredado por todos los controladores
- **Route constraints**: `{id:int}`, `{priority:alpha}`
- **Sub-recursos**: `/api/Tickets/{id}/comments`, `/api/Tickets/{id}/close`
- **Ruta custom**: `TicketCommentsController` usa `[Route("api/Tickets/{ticketId:int}/comments")]` que sobreescribe el base

### HTTP Status Codes
- `200 OK` — Éxito en GET, PUT, DELETE
- `201 Created` — Éxito en POST
- `400 Bad Request` — Error de validación o negocio
- `404 Not Found` — Recurso no encontrado
- `500 Internal Server Error` — Error inesperado del servidor

### Response Format
```json
// Éxito
{
  "ticketId": "...",
  "ticketNumber": 1042,
  "status": "Open"
}

// Error
{
  "error": "Error message describing the issue"
}
```

---

## 🚀 Ejecución

### Desarrollo
```bash
cd HelpDesk.API
dotnet run
```

### Watch mode (hot reload)
```bash
dotnet watch run
```

### Producción
```bash
dotnet publish -c Release
dotnet HelpDesk.API.dll
```

### URLs por defecto
- **HTTP**: `http://localhost:5000`
- **HTTPS**: `https://localhost:5001`
- **OpenAPI**: `https://localhost:5001/openapi/v1.json` (solo dev)

---

## 🧪 Testing con curl

### Crear Ticket
```bash
curl -X POST https://localhost:5001/api/Tickets \
  -H "Content-Type: application/json" \
  -d '{
    "departmentId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "supportTypeId": "7cb96a31-1234-4bcd-a987-3d9c4f77bc12",
    "priority": "High",
    "subject": "Error en sistema de nómina",
    "description": "No puedo acceder desde esta mañana."
  }'
```

### Obtener Ticket por ID
```bash
curl -X GET https://localhost:5001/api/Tickets/{guid}
```

### Cambiar Estado a En Proceso
```bash
curl -X PUT https://localhost:5001/api/Tickets/{guid}/status \
  -H "Content-Type: application/json" \
  -d '{ "newStatus": "InProgress" }'
```

### Cerrar Ticket
```bash
curl -X PUT https://localhost:5001/api/Tickets/{guid}/close \
  -H "Content-Type: application/json" \
  -d '{
    "resolutionCategory": "Resolved",
    "closingComment": "Problema resuelto exitosamente."
  }'
```

### Actualizar SLA de Prioridad Alta
```bash
curl -X PUT https://localhost:5001/api/SlaConfigurations/High \
  -H "Content-Type: application/json" \
  -d '{ "hoursLimit": 6 }'
```

### Asignar agente activo a tipo de soporte (UserId viene del token)
```bash
curl -X POST https://localhost:5001/api/SupportTypeAgents \
  -H "Content-Type: application/json" \
  -d '{ "supportTypeId": "7cb96a31-1234-4bcd-a987-3d9c4f77bc12" }'
```

### Dashboard — Vista general del usuario autenticado
```bash
curl -X GET https://localhost:5001/api/Dashboard
```

### Dashboard — Métricas operativas del mes
```bash
curl -X GET "https://localhost:5001/api/Dashboard/metrics?from=2026-04-01&to=2026-04-30"
```

### Dashboard — Mapa de calor
```bash
curl -X GET "https://localhost:5001/api/Dashboard/heatmap?from=2026-04-01&to=2026-04-30"
```

### Dashboard — Rendimiento de agente
```bash
curl -X GET "https://localhost:5001/api/Dashboard/agents/jlopez/performance?from=2026-04-01&to=2026-04-30"
```

### Dashboard — Ranking mensual
```bash
curl -X GET "https://localhost:5001/api/Dashboard/leaderboard?year=2026&month=4"
```

### Health Check
```bash
curl -X GET https://localhost:5001/health
```

---

## 💡 Mejores Prácticas

1. **Controladores delgados**: Toda la lógica de negocio en servicios
2. **Usar DTOs**: Nunca exponer entidades de dominio en la respuesta
3. **Validación en Application**: No duplicar validaciones en controladores
4. **CancellationToken**: Siempre en métodos async
5. **Result Pattern**: Mapear `IsFailure` a códigos HTTP apropiados
6. **Logging**: Usar `ILogger` para diagnósticos y trazabilidad
7. **CORS específico**: No usar `AllowAll` en producción
8. **HTTPS**: Forzar redirección en producción
9. **Rate limiting**: Agregar para endpoints públicos o de escritura masiva
10. **Versionado**: Preparar para API v2, v3 al escalar funcionalidades

---

## 🔒 Seguridad (TODO)

### Agregar Autenticación JWT
```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => { ... });

app.UseAuthentication();
app.UseAuthorization();
```

### Agregar Autorización por Rol
```csharp
[Authorize(Roles = "Administrator")]
[HttpPut("{priority}")]
public async Task<IActionResult> UpdateSla(...)

[Authorize(Roles = "SupportAgent,Coordinator,Administrator")]
[HttpPut("{id:int}/close")]
public async Task<IActionResult> Close(...)
```

### Rate Limiting
```csharp
builder.Services.AddRateLimiter(options => { ... });
app.UseRateLimiter();
```

---

## 📊 Monitoreo y Observabilidad (TODO)

### Application Insights
```csharp
builder.Services.AddApplicationInsightsTelemetry();
```

### Health Checks Avanzados
```csharp
builder.Services.AddHealthChecks()
    .AddDbContextCheck<HelpDeskDbContext>()
    .AddSqlServer(connectionString);

app.MapHealthChecks("/health");
```

### Logging
```csharp
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddApplicationInsights();
```

---

## 🎓 Conceptos Aplicados

- **RESTful API Design**
- **Clean Architecture** (Capa de presentación)
- **Dependency Injection**
- **ASP.NET Core Middleware Pipeline**
- **OpenAPI Specification**
- **HTTP Semantics**

---

## 📌 Notas Importantes

- Este es el **único proyecto ejecutable** de la solución HelpDesk
- Actúa como **Composition Root** para toda la aplicación
- **No debe contener lógica de negocio**, solo coordinar capas
- La configuración en Program.cs **conecta todas las capas**
- OpenAPI solo se expone en **Development** por seguridad
- CORS está en modo **AllowAll** (cambiar en producción)
- Autenticación/Autorización está **pendiente de implementar**

---

## 🚀 Próximos Pasos

1. ✅ Implementar autenticación JWT con roles (Requester, SupportAgent, Coordinator, Administrator)
2. ✅ Agregar autorización por rol en cada endpoint
3. ✅ Configurar rate limiting
4. ✅ Agregar health checks avanzados con verificación de BD
5. ✅ Implementar versionado de API
6. ✅ Configurar Application Insights
7. ✅ Implementar caché (Redis) para catálogos de departamentos y tipos de soporte
8. ✅ Agregar documentación Swagger extendida con ejemplos por endpoint
9. ✅ Configurar CORS específico para producción
10. ✅ Implementar notificaciones push/email como middleware o background service
11. ✅ Agregar filtrado por rol en DashboardController mediante claim del token
12. ✅ Implementar caché (Redis) para HeatMapDto y LeaderboardDto (datos costosos y poco volátiles)
