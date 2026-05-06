# HelpDesk.Application

## 📋 Descripción General

**HelpDesk.Application** es la capa de aplicación del proyecto HelpDesk, responsable de orquestar la lógica de negocio y coordinar las operaciones entre la capa de presentación (API) y la capa de dominio. Implementa los casos de uso del sistema de gestión de tickets y maneja la transformación de datos entre DTOs y entidades de dominio.

---

## 🎯 Propósito

Este proyecto proporciona:
- **Casos de uso (Use Cases)**: Implementación de las operaciones de negocio de tickets
- **DTOs (Data Transfer Objects)**: Modelos para transferencia de datos entre capas
- **Servicios de aplicación**: Lógica de orquestación por entidad del dominio
- **Validaciones**: Reglas de validación con FluentValidation
- **Mapeos**: Transformación entre DTOs y entidades con AutoMapper
- **Result Pattern**: Manejo funcional de resultados y errores

---

## 🏗️ Arquitectura

### Patrón Arquitectónico
- **Clean Architecture** — Capa de aplicación
- **Service Layer Pattern**: Servicios que encapsulan casos de uso
- **DTO Pattern**: Separación entre modelos de dominio y de transferencia
- **Result Pattern**: Manejo funcional de éxito/error

### Principios Aplicados
- ✅ **Use Case Driven**: Cada servicio representa casos de uso concretos del dominio HelpDesk
- ✅ **Validation First**: Validación explícita antes de toda operación
- ✅ **DTO Mapping**: Transformación automática con AutoMapper
- ✅ **Functional Error Handling**: Result Pattern en lugar de excepciones

---

## 📦 Dependencias

### Proyectos Referenciados
```
HelpDesk.Application
    └── HelpDesk.Domain (Capa de Dominio)
```

### Paquetes NuGet

| Paquete | Versión | Propósito |
|---------|---------|-----------|
| `AutoMapper` | 16.1.1 | Mapeo automático de objetos (DI incluido desde v16) |
| `FluentValidation.DependencyInjectionExtensions` | 12.1.1 | Validaciones fluidas |

### Framework
- **.NET 10.0**
- **Nullable Reference Types**: Habilitado
- **Implicit Usings**: Habilitado

---

## 📁 Estructura del Proyecto

```
HelpDesk.Application/
├── Common/
│   └── Result.cs                                # Result Pattern implementation
│       ├── Result<T>                            # Para operaciones con valor de retorno
│       └── Result                               # Para operaciones void
│
├── DTOs/                                        # Data Transfer Objects
│   ├── Ticket/
│   │   ├── TicketDto.cs
│   │   ├── TicketSummaryDto.cs
│   │   ├── CreateTicketRequest.cs
│   │   ├── UpdateTicketStatusRequest.cs
│   │   └── CloseTicketRequest.cs
│   ├── TicketComment/
│   │   ├── TicketCommentDto.cs
│   │   └── AddCommentRequest.cs
│   ├── TicketAttachment/
│   │   └── TicketAttachmentDto.cs
│   ├── Department/
│   │   ├── DepartmentDto.cs
│   │   └── CreateDepartmentRequest.cs
│   ├── SupportType/
│   │   ├── SupportTypeDto.cs
│   │   └── CreateSupportTypeRequest.cs
│   ├── SupportTypeAgent/
│   │   ├── SupportTypeAgentDto.cs               # Detalle de asignación activa
│   │   └── AssignAgentRequest.cs                # UserId extraído del token en la capa API
│   ├── SlaConfiguration/
│   │   ├── SlaConfigurationDto.cs
│   │   └── UpdateSlaConfigurationRequest.cs
│   ├── Score/
│   │   ├── UserScoreDto.cs
│   │   └── ScoreTransactionDto.cs
│   └── Dashboard/
│       ├── RequesterDashboardDto.cs             # Vista del solicitante
│       ├── AgentDashboardDto.cs                 # Vista del agente de soporte
│       ├── CoordinatorDashboardDto.cs           # Vista del coordinador / supervisor
│       ├── AdminDashboardDto.cs                 # Vista del administrador
│       ├── OperationalMetricsDto.cs             # KPIs operativos (US-22)
│       ├── AgentPerformanceDto.cs               # Rendimiento individual del agente (US-23)
│       ├── HeatMapDto.cs                        # Mapa de calor depto × tipo de soporte (US-24)
│       └── LeaderboardDto.cs                    # Ranking mensual de reputación (US-18)
│
├── Services/                                    # Servicios de aplicación
│   ├── TicketService.cs                         # Ciclo de vida completo del ticket
│   ├── TicketCommentService.cs                  # Gestión de comentarios e hilo
│   ├── DepartmentService.cs                     # Administración de departamentos
│   ├── SupportTypeService.cs                    # Administración de tipos de soporte
│   ├── SupportTypeAgentService.cs               # Asignación de agentes a tipos de soporte
│   ├── SlaConfigurationService.cs               # Configuración de tiempos SLA
│   ├── ScoreService.cs                     # Sistema de puntos y niveles
│   └── DashboardService.cs                      # Métricas y vistas por rol de usuario
│
├── Interfaces/                                  # Contratos de servicios
│   ├── ITicketService.cs
│   ├── ITicketCommentService.cs
│   ├── IDepartmentService.cs
│   ├── ISupportTypeService.cs
│   ├── ISupportTypeAgentService.cs
│   ├── ISlaConfigurationService.cs
│   ├── IScoreService.cs
│   └── IDashboardService.cs
│
├── Validators/                                  # FluentValidation validators
│   ├── CreateTicketRequestValidator.cs
│   ├── AddCommentRequestValidator.cs
│   ├── CloseTicketRequestValidator.cs
│   ├── UpdateTicketStatusRequestValidator.cs
│   ├── CreateDepartmentRequestValidator.cs
│   ├── CreateSupportTypeRequestValidator.cs
│   ├── AssignAgentRequestValidator.cs
│   └── UpdateSlaConfigurationRequestValidator.cs
│
└── Mappings/                                    # Perfiles de AutoMapper
    └── MappingProfile.cs                        # Mapeos DTO ↔ Entity
```

---

## 🔑 Componentes Clave

### 1. Result Pattern

#### **Result\<T\>** — Para operaciones con valor de retorno
```csharp
public class Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure { get; }
    public T Value { get; }
    public string Error { get; }

    // Factory methods
    public static Result<T> Success(T value);
    public static Result<T> Failure(string error);

    // Functional methods
    public Result<TNew> Map<TNew>(Func<T, TNew> mapper);
    public Result<TNew> Bind<TNew>(Func<T, Result<TNew>> binder);
    public Result<T> Match(Action<T> onSuccess, Action<string> onFailure);
}
```

#### **Result** — Para operaciones void
```csharp
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure { get; }
    public string Error { get; }

    public static Result Success();
    public static Result Failure(string error);
}
```

**Ventajas:**
- ✅ No usa excepciones para flujo de control
- ✅ Hace explícito el éxito/error en cada operación
- ✅ Permite composición funcional (Map, Bind)
- ✅ Railway-Oriented Programming

---

### 2. Servicios de Aplicación

#### **ITicketService**
```csharp
public interface ITicketService
{
    Task<Result<TicketDto>> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<Result<IEnumerable<TicketSummaryDto>>> GetAllAsync(CancellationToken cancellationToken);
    Task<Result<IEnumerable<TicketSummaryDto>>> GetByAgentAsync(string agentUserId, CancellationToken cancellationToken);
    Task<Result<TicketDto>> CreateAsync(CreateTicketRequest request, string createdBy, CancellationToken cancellationToken);
    Task<Result<TicketDto>> ChangeStatusAsync(int id, UpdateTicketStatusRequest request, string changedBy, CancellationToken cancellationToken);
    Task<Result<TicketDto>> CloseAsync(int id, CloseTicketRequest request, string closedBy, CancellationToken cancellationToken);
    Task<Result<TicketDto>> ReopenAsync(int id, string reason, string reopenedBy, CancellationToken cancellationToken);
    Task<Result<TicketDto>> RedirectAsync(int id, int newSupportTypeId, string redirectedBy, CancellationToken cancellationToken);
    Task<Result> DeleteAsync(int id, string deletedBy, CancellationToken cancellationToken);
}
```

#### **TicketService**
Implementación que orquesta:
1. **Valida** la entrada con FluentValidation
2. **Calcula** la fecha límite SLA al crear un ticket
3. **Registra** la entrada en el historial de auditoría en cada cambio de estado
4. **Aplica** la penalización de reputación al redirigir o cerrar con resolución negativa
5. **Pausa y reanuda** el SLA al cambiar al estado `WaitingForInfo`
6. **Retorna** `Result<T>` para manejo funcional de errores

#### **IScoreService**
```csharp
public interface IScoreService
{
    Task<Result<UserScoreDto>> GetByUserIdAsync(string userId, CancellationToken cancellationToken);
    Task<Result> ApplyRatingPointsAsync(int ticketId, string userId, bool hasComment, CancellationToken cancellationToken);
    Task<Result> ApplyPenaltyAsync(int ticketId, string userId, ScoreTransactionReason reason, CancellationToken cancellationToken);
}
```

#### **ISupportTypeAgentService**
Gestiona la asignación de agentes a tipos de soporte. El `UserId` siempre proviene del token JWT, extraído en la capa API y pasado como parámetro explícito — Application nunca accede al token directamente.

```csharp
public interface ISupportTypeAgentService
{
    /// <summary>Retorna la asignación activa (IsEnabled = true) de un tipo de soporte.</summary>
    Task<Result<SupportTypeAgentDto>> GetActiveAgentAsync(int supportTypeId, CancellationToken cancellationToken);

    /// <summary>Retorna el historial completo de asignaciones (activas e inactivas) de un tipo de soporte.</summary>
    Task<Result<IEnumerable<SupportTypeAgentDto>>> GetHistoryAsync(int supportTypeId, CancellationToken cancellationToken);

    /// <summary>
    /// Crea una nueva asignación activa. Realiza soft delete del registro activo anterior
    /// antes de insertar el nuevo, preservando el historial de cambios.
    /// assignedBy: userId extraído del token en el controlador.
    /// </summary>
    Task<Result<SupportTypeAgentDto>> AssignAsync(AssignAgentRequest request, string assignedBy, CancellationToken cancellationToken);

    /// <summary>
    /// Desactiva la asignación activa (soft delete) sin crear una nueva.
    /// Deja el tipo de soporte sin agente hasta una nueva asignación.
    /// </summary>
    Task<Result> UnassignAsync(int supportTypeId, string unassignedBy, CancellationToken cancellationToken);
}
```

#### **SupportTypeAgentService** — Implementación destacada
Orquesta la asignación dentro de una transacción atómica:
1. **Verifica** que el `SupportTypeId` existe y está activo
2. **Soft delete** del registro activo previo: `IsEnabled = false`, registra `DisabledAt` y `DisabledBy`
3. **Inserta** el nuevo registro con `UserId` del token y `IsEnabled = true`
4. **Retorna** el `SupportTypeAgentDto` con los datos del registro creado

```csharp
public async Task<Result<SupportTypeAgentDto>> AssignAsync(
    AssignAgentRequest request,
    string assignedBy,
    CancellationToken cancellationToken)
{
    var supportType = await _unitOfWork.SupportTypes
        .GetByIdAsync(request.SupportTypeId, cancellationToken);

    if (supportType is null)
        return Result<SupportTypeAgentDto>.Failure("Support type not found");

    await _unitOfWork.BeginTransactionAsync();
    try
    {
        // Soft delete del registro activo anterior (si existe)
        var currentActive = await _unitOfWork.SupportTypeAgents
            .FirstOrDefaultAsync(
                a => a.SupportTypeId == request.SupportTypeId && a.IsEnabled,
                cancellationToken);

        if (currentActive is not null)
        {
            currentActive.IsEnabled  = false;
            currentActive.DisabledAt = DateTime.UtcNow;
            currentActive.DisabledBy = assignedBy;
            _unitOfWork.SupportTypeAgents.Update(currentActive);
        }

        // Crear nueva asignación activa — UserId viene del token, nunca hardcodeado
        // SupportTypeAgentId no se asigna: es INT IDENTITY, lo genera la BD
        var newAssignment = new SupportTypeAgent
        {
            SupportTypeId = request.SupportTypeId,
            UserId        = request.UserId,   // extraído del token en el controlador
            IsEnabled     = true,
            CreatedAt     = DateTime.UtcNow,
            CreatedBy     = assignedBy
        };

        await _unitOfWork.SupportTypeAgents.AddAsync(newAssignment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync();

        return Result<SupportTypeAgentDto>.Success(_mapper.Map<SupportTypeAgentDto>(newAssignment));
    }
    catch
    {
        await _unitOfWork.RollbackTransactionAsync();
        throw;
    }
}
```

#### **IDashboardService**
Provee métricas y vistas consolidadas para cada rol. Todos los métodos reciben el `userId` del token para filtrar el alcance (tickets propios, bandeja del agente, departamento del coordinador, o sistema completo). El controlador extrae los claims y los pasa; Application nunca toca el token.

```csharp
public interface IDashboardService
{
    // ─── US-21: Vista general por rol ──────────────────────────────────────────

    /// <summary>Tickets activos agrupados por estado + nivel de reputación actual.</summary>
    Task<Result<RequesterDashboardDto>> GetRequesterDashboardAsync(
        string userId, CancellationToken cancellationToken);

    /// <summary>
    /// Bandeja del agente ordenada por urgencia SLA (rojo→amarillo→verde)
    /// + carga de trabajo + tickets sin actividad > 24h.
    /// </summary>
    Task<Result<AgentDashboardDto>> GetAgentDashboardAsync(
        string userId, CancellationToken cancellationToken);

    /// <summary>
    /// Estado del departamento bajo supervisión + carga por agente
    /// + escalamientos activos pendientes.
    /// </summary>
    Task<Result<CoordinatorDashboardDto>> GetCoordinatorDashboardAsync(
        string userId, CancellationToken cancellationToken);

    /// <summary>
    /// Vista global: tickets activos en todos los departamentos,
    /// % cumplimiento SLA del día, distribución por prioridad.
    /// </summary>
    Task<Result<AdminDashboardDto>> GetAdminDashboardAsync(CancellationToken cancellationToken);

    // ─── US-22: Métricas operativas ────────────────────────────────────────────

    /// <summary>
    /// KPIs del período: volumen, tasa SLA, tiempos promedio, tasa de reapertura y redirección.
    /// departmentId null = todos los departamentos (solo Administrador).
    /// </summary>
    Task<Result<OperationalMetricsDto>> GetOperationalMetricsAsync(
        DateTime from, DateTime to, int? departmentId, CancellationToken cancellationToken);

    // ─── US-23: Rendimiento individual del agente ───────────────────────────────

    /// <summary>
    /// KPIs individuales + comparativa con el promedio del equipo.
    /// El agente solo puede consultar su propio userId;
    /// el coordinador puede consultar cualquier agente de su departamento.
    /// </summary>
    Task<Result<AgentPerformanceDto>> GetAgentPerformanceAsync(
        string agentUserId, DateTime from, DateTime to, CancellationToken cancellationToken);

    /// <summary>Tabla comparativa de todos los agentes de un departamento.</summary>
    Task<Result<IEnumerable<AgentPerformanceDto>>> GetTeamPerformanceAsync(
        int departmentId, DateTime from, DateTime to, CancellationToken cancellationToken);

    // ─── US-24: Mapa de calor ──────────────────────────────────────────────────

    /// <summary>
    /// Matriz departamento × tipo de soporte con volumen e incumplimiento de SLA por celda.
    /// departmentIds null = todos (Administrador).
    /// El coordinador recibe automáticamente solo sus departamentos.
    /// </summary>
    Task<Result<HeatMapDto>> GetHeatMapAsync(
        DateTime from, DateTime to, IEnumerable<int>? departmentIds, CancellationToken cancellationToken);

    // ─── US-18: Ranking mensual ────────────────────────────────────────────────

    /// <summary>Top 10 usuarios por puntos GANADOS en el mes + tasa de calificación.</summary>
    Task<Result<LeaderboardDto>> GetMonthlyLeaderboardAsync(
        int year, int month, CancellationToken cancellationToken);
}
```

---

### 3. DTOs (Data Transfer Objects)

#### **TicketDto** — Output completo
```csharp
public class TicketDto
{
    public int TicketId { get; set; }
    public int TicketNumber { get; set; }
    public string Subject { get; set; }
    public string Description { get; set; }
    public string DepartmentName { get; set; }
    public string SupportTypeName { get; set; }
    public string Priority { get; set; }
    public string Status { get; set; }
    public string? ResolutionCategory { get; set; }
    public string? AssignedUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; }
    public DateTime? FirstOpenedAt { get; set; }
    public DateTime? WorkStartedAt { get; set; }
    public DateTime Deadline { get; set; }
    public DateTime? ClosedAt { get; set; }
    public int TotalPausedMinutes { get; set; }
    public List<TicketCommentDto> Comments { get; set; }
    public List<TicketAttachmentDto> Attachments { get; set; }
}
```

#### **TicketSummaryDto** — Output para bandeja
```csharp
public class TicketSummaryDto
{
    public int TicketId { get; set; }
    public int TicketNumber { get; set; }
    public string Subject { get; set; }
    public string Priority { get; set; }
    public string Status { get; set; }
    public string DepartmentName { get; set; }
    public string SupportTypeName { get; set; }
    public string? AssignedUserId { get; set; }
    public DateTime Deadline { get; set; }
    public double RemainingSlaPct { get; set; }    // % tiempo SLA restante
}
```

#### **CreateTicketRequest** — Input de creación
```csharp
public class CreateTicketRequest
{
    public int DepartmentId { get; set; }
    public int SupportTypeId { get; set; }
    public string Priority { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
```

#### **CloseTicketRequest** — Input de cierre
```csharp
public class CloseTicketRequest
{
    public string ResolutionCategory { get; set; }
    public string? ClosingComment { get; set; }
}
```

#### **SupportTypeAgentDto** — Output de asignación
```csharp
public class SupportTypeAgentDto
{
    public int SupportTypeAgentId { get; set; }
    public int SupportTypeId { get; set; }
    public string SupportTypeName { get; set; }
    public string UserId { get; set; }         // Identificador del agente (del token)
    public bool IsEnabled { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; }
    public DateTime? DisabledAt { get; set; }
    public string? DisabledBy { get; set; }
}
```

#### **AssignAgentRequest** — Input de asignación
```csharp
public class AssignAgentRequest
{
    public int SupportTypeId { get; set; }
    public string UserId { get; set; } = string.Empty;  // Extraído del token JWT en el controlador
}
```

#### **RequesterDashboardDto** — Vista del solicitante (US-21)
```csharp
public class RequesterDashboardDto
{
    public IEnumerable<TicketSummaryDto> ActiveTickets { get; set; }
    public Dictionary<string, int> TicketCountByStatus { get; set; }  // "Open"→2, "InProgress"→1
    public UserScoreDto Score { get; set; }
}
```

#### **AgentDashboardDto** — Vista del agente (US-21)
```csharp
public class AgentDashboardDto
{
    public IEnumerable<TicketSummaryDto> Queue { get; set; }    // Ordenada por urgencia SLA
    public int TotalAssigned { get; set; }
    public int OverdueCount { get; set; }
    public int StaleCount { get; set; }                          // Sin actividad > 24h
    public double WorkloadPct { get; set; }                      // % sobre capacidad configurada
}
```

#### **CoordinatorDashboardDto** — Vista del coordinador (US-21)
```csharp
public class CoordinatorDashboardDto
{
    public string DepartmentName { get; set; }
    public int TotalActive { get; set; }
    public int SlaBreachedCount { get; set; }
    public IEnumerable<AgentWorkloadDto> TeamWorkload { get; set; }
    public IEnumerable<TicketSummaryDto> EscalatedTickets { get; set; }
}

public class AgentWorkloadDto
{
    public string UserId { get; set; }
    public int AssignedCount { get; set; }
    public int OverdueCount { get; set; }
}
```

#### **AdminDashboardDto** — Vista del administrador (US-21)
```csharp
public class AdminDashboardDto
{
    public int TotalActiveAllDepartments { get; set; }
    public double SlaDailyCompliancePct { get; set; }
    public Dictionary<string, int> CountByPriority { get; set; }
    public Dictionary<string, int> CountByDepartment { get; set; }
}
```

#### **OperationalMetricsDto** — KPIs operativos (US-22)
```csharp
public class OperationalMetricsDto
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public int TotalCreated { get; set; }
    public int TotalClosed { get; set; }
    public int TotalActive { get; set; }
    public double SlaCompliancePct { get; set; }
    public double AvgResolutionHours { get; set; }
    public double AvgFirstResponseHours { get; set; }
    public double ReopenRatePct { get; set; }
    public double RedirectRatePct { get; set; }
    public IEnumerable<DailyVolumeDto> DailyTrend { get; set; }
    public Dictionary<string, double> SlaByPriority { get; set; }
    // Comparativa opcional con período anterior
    public OperationalMetricsDto? PreviousPeriod { get; set; }
}

public class DailyVolumeDto
{
    public DateOnly Date { get; set; }
    public int Created { get; set; }
    public int Closed { get; set; }
}
```

#### **AgentPerformanceDto** — Rendimiento individual (US-23)
```csharp
public class AgentPerformanceDto
{
    public string UserId { get; set; }
    public int TotalAssigned { get; set; }
    public int TotalClosed { get; set; }
    public int TotalActive { get; set; }
    public double SlaCompliancePct { get; set; }
    public double AvgResolutionHours { get; set; }
    public double AvgFirstResponseHours { get; set; }
    public int PauseCount { get; set; }
    public double AvgPauseMinutes { get; set; }
    public double AvgRating { get; set; }                   // Solo sobre tickets calificados
    // Comparativa vs promedio del equipo
    public double TeamAvgSlaCompliancePct { get; set; }
    public double TeamAvgResolutionHours { get; set; }
    public IEnumerable<TicketSummaryDto> StaleTickets { get; set; }  // Sin actividad > 48h
}
```

#### **HeatMapDto** — Mapa de calor (US-24)
```csharp
public class HeatMapDto
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public IEnumerable<HeatMapRowDto> Rows { get; set; }
}

public class HeatMapRowDto
{
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; }
    public IEnumerable<HeatMapCellDto> Cells { get; set; }
}

public class HeatMapCellDto
{
    public int SupportTypeId { get; set; }
    public string SupportTypeName { get; set; }
    public int Volume { get; set; }
    public double SlaBreachPct { get; set; }    // Para la capa de SLA (verde/amarillo/rojo)
}
```

#### **LeaderboardDto** — Ranking mensual (US-18)
```csharp
public class LeaderboardDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public IEnumerable<LeaderboardEntryDto> Top10 { get; set; }
}

public class LeaderboardEntryDto
{
    public int Rank { get; set; }
    public string UserId { get; set; }
    public int PointsEarned { get; set; }       // Solo transacciones positivas del mes
    public double RatingRatePct { get; set; }   // Tickets calificados / Tickets cerrados × 100
}
```

**Propósito de DTOs:**
- 🔒 **Encapsulación**: No expone entidades de dominio directamente
- 🎯 **Específicos**: Un DTO por caso de uso (resumen vs. detalle completo)
- ✨ **Versionables**: Fácil agregar/remover campos sin romper el dominio
- 🚀 **Optimizados**: Solo los datos necesarios para cada operación

---

### 4. Validadores (FluentValidation)

#### **CreateTicketRequestValidator**
```csharp
public class CreateTicketRequestValidator : AbstractValidator<CreateTicketRequest>
{
    public CreateTicketRequestValidator()
    {
        RuleFor(x => x.DepartmentId)
            .NotEmpty()
            .WithMessage("Department is required");

        RuleFor(x => x.SupportTypeId)
            .NotEmpty()
            .WithMessage("Support type is required");

        RuleFor(x => x.Priority)
            .NotEmpty()
            .Must(p => Enum.TryParse<TicketPriority>(p, out _))
            .WithMessage("Priority must be a valid value: Critical, High, Medium, Low");

        RuleFor(x => x.Subject)
            .NotEmpty()
            .MaximumLength(255)
            .WithMessage("Subject is required and must not exceed 255 characters");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required");
    }
}
```

#### **AddCommentRequestValidator**
```csharp
public class AddCommentRequestValidator : AbstractValidator<AddCommentRequest>
{
    public AddCommentRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Comment content is required when changing to WaitingForInfo");

        RuleFor(x => x.Visibility)
            .Must(v => Enum.TryParse<CommentVisibility>(v, out _))
            .WithMessage("Visibility must be Public or Internal");
    }
}
```

**Características:**
- ✅ **Fluent API**: Sintaxis declarativa y legible
- ✅ **Composable**: Validadores reutilizables entre requests
- ✅ **Testeable**: Fácil de probar de forma aislada
- ✅ **Mensajes personalizados**: Errores claros para el cliente

---

### 5. AutoMapper

#### **MappingProfile**
```csharp
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Ticket mappings
        CreateMap<Ticket, TicketDto>()
            .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Department.Name))
            .ForMember(dest => dest.SupportTypeName, opt => opt.MapFrom(src => src.SupportType.Name))
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.ResolutionCategory, opt => opt.MapFrom(src =>
                src.ResolutionCategory.HasValue ? src.ResolutionCategory.ToString() : null));

        CreateMap<Ticket, TicketSummaryDto>()
            .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Department.Name))
            .ForMember(dest => dest.SupportTypeName, opt => opt.MapFrom(src => src.SupportType.Name))
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        CreateMap<CreateTicketRequest, Ticket>()
            .ForMember(dest => dest.TicketId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Deadline, opt => opt.Ignore());

        // Comment mappings
        CreateMap<TicketComment, TicketCommentDto>()
            .ForMember(dest => dest.Visibility, opt => opt.MapFrom(src => src.Visibility.ToString()));

        // Score mappings
        CreateMap<UserScore, UserScoreDto>()
            .ForMember(dest => dest.Level, opt => opt.MapFrom(src => src.Level.ToString()));
        CreateMap<ScoreTransaction, ScoreTransactionDto>()
            .ForMember(dest => dest.Reason, opt => opt.MapFrom(src => src.Reason.ToString()));

        // Department / SupportType mappings
        CreateMap<Department, DepartmentDto>();
        CreateMap<CreateDepartmentRequest, Department>()
            .ForMember(dest => dest.DepartmentId, opt => opt.Ignore());

        CreateMap<SupportType, SupportTypeDto>();
        CreateMap<CreateSupportTypeRequest, SupportType>()
            .ForMember(dest => dest.SupportTypeId, opt => opt.Ignore());

        // SlaConfiguration mappings
        CreateMap<SlaConfiguration, SlaConfigurationDto>()
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority.ToString()));

        // SupportTypeAgent mappings
        CreateMap<SupportTypeAgent, SupportTypeAgentDto>()
            .ForMember(dest => dest.SupportTypeName, opt => opt.MapFrom(src => src.SupportType.Name));
    }
}
```

---

## 🔄 Flujo de Datos

```
Controller (HelpDesk.API)
    ↓ DTO Request
Service (HelpDesk.Application)
    ↓ Validación (FluentValidation)
    ↓ Map to Entity (AutoMapper)
Repository (HelpDesk.Infrastructure via IUnitOfWork)
    ↓ Entity
Database
    ↑ Entity
Repository
    ↑ Entity
Service
    ↑ Map to DTO (AutoMapper)
    ↑ Result<DTO>
Controller
    ↑ JSON Response
```

---

## 🎨 Patrones Utilizados

### 1. **Service Layer Pattern**
- Servicios encapsulan los casos de uso del dominio HelpDesk
- Un servicio por agregado (`TicketService`, `ScoreService`, etc.)

### 2. **DTO Pattern**
- Separación entre modelos internos y externos
- Versiones de respuesta diferenciadas (ej: `TicketDto` vs `TicketSummaryDto`)

### 3. **Result Pattern** (Railway-Oriented Programming)
```csharp
// Composición funcional
var result = await GetTicketAsync(id, cancellationToken)
    .Map(ticket => ticket.Status)
    .Bind(status => ValidateTransition(status, newStatus));
```

### 4. **Validation Pattern**
- FluentValidation para reglas de negocio complejas
- Validación obligatoria antes de toda operación de escritura

### 5. **Dependency Injection**
- Servicios registrados en el contenedor IoC
- Interfaces para testabilidad e intercambiabilidad

---

## ✨ Características Destacadas

### Cálculo de SLA al Crear Ticket

```csharp
public async Task<Result<TicketDto>> CreateAsync(
    CreateTicketRequest request,
    string createdBy,
    CancellationToken cancellationToken)
{
    // Validación
    var validationResult = await _validator.ValidateAsync(request, cancellationToken);
    if (!validationResult.IsValid)
        return Result<TicketDto>.Failure(string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));

    // Obtener configuración SLA de la prioridad
    var slaConfig = await _unitOfWork.SlaConfigurations
        .FirstOrDefaultAsync(s => s.Priority == Enum.Parse<TicketPriority>(request.Priority), cancellationToken);

    if (slaConfig is null)
        return Result<TicketDto>.Failure("SLA configuration not found for the selected priority");

    // Mapeo y cálculo de deadline
    var ticket = _mapper.Map<Ticket>(request);
    ticket.CreatedAt = DateTime.UtcNow;
    ticket.CreatedBy = createdBy;
    ticket.Status = TicketStatus.Open;
    ticket.Deadline = DateTime.UtcNow.AddHours(slaConfig.HoursLimit);

    // Asignar agente activo del tipo de soporte via SupportTypeAgent
    var activeAssignment = await _unitOfWork.SupportTypeAgents
        .FirstOrDefaultAsync(a => a.SupportTypeId == request.SupportTypeId && a.IsEnabled, cancellationToken);
    ticket.AssignedUserId = activeAssignment?.UserId;  // null si el tipo de soporte no tiene agente activo

    await _unitOfWork.Tickets.AddAsync(ticket, cancellationToken);
    await _unitOfWork.SaveChangesAsync(cancellationToken);

    var dto = _mapper.Map<TicketDto>(ticket);
    return Result<TicketDto>.Success(dto);
}
```

### Pausa y Reanudación de SLA

```csharp
// Al cambiar a WaitingForInfo
ticket.PausedAt = DateTime.UtcNow;
ticket.Status = TicketStatus.WaitingForInfo;

// Al responder el usuario
var pausedMinutes = (int)(DateTime.UtcNow - ticket.PausedAt!.Value).TotalMinutes;
ticket.TotalPausedMinutes += pausedMinutes;
ticket.Deadline = ticket.Deadline.AddMinutes(pausedMinutes);
ticket.PausedAt = null;
ticket.Status = TicketStatus.InProgress;
```

---

## 🔗 Proyectos que Dependen de Este

- **HelpDesk.API** ← Usa los servicios e interfaces

---

## 🔗 Dependencias de Este Proyecto

- **HelpDesk.Domain** ← Usa entidades e interfaces de repositorio

---

## 📝 Convenciones

### Naming
- **Servicios**: `{Entity}Service` (ej: `TicketService`, `ScoreService`)
- **Interfaces**: `I{Entity}Service` (ej: `ITicketService`)
- **DTOs de salida**: `{Entity}Dto` o `{Entity}SummaryDto`
- **DTOs de entrada**: `{Action}{Entity}Request`
- **Validators**: `{Request}Validator`

### Organización
- **Un servicio por agregado de dominio**
- **DTOs agrupados por entidad**
- **Validadores junto a los requests que validan**

---

## 🚀 Uso

### Registrar Servicios (en Program.cs)
```csharp
// AutoMapper — DI incluido en el paquete principal desde v16
builder.Services.AddAutoMapper(cfg => cfg.AddMaps(typeof(MappingProfile).Assembly));

// FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(MappingProfile).Assembly);

// Servicios de aplicación
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<ITicketCommentService, TicketCommentService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<ISupportTypeService, SupportTypeService>();
builder.Services.AddScoped<ISlaConfigurationService, SlaConfigurationService>();
builder.Services.AddScoped<IScoreService, ScoreService>();
builder.Services.AddScoped<ISupportTypeAgentService, SupportTypeAgentService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
```

### Usar en Controladores (con BaseApiController)
```csharp
// Todos los controladores heredan BaseApiController — no necesitan [ApiController], [Route] ni [Produces]
public class TicketsController : BaseApiController
{
    private readonly ITicketService _ticketService;

    public TicketsController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _ticketService.GetByIdAsync(id, cancellationToken);
        return HandleGetResult(result);  // 200 OK / 404 Not Found
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateTicketRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _ticketService.CreateAsync(request, GetCurrentUser(), cancellationToken);
        return HandleCreateResult(result, nameof(GetById), new { id = result.Value?.TicketId });
    }
}
```

---

## 💡 Mejores Prácticas

1. **Siempre usar DTOs**, nunca exponer entidades de dominio
2. **Validar antes de procesar** con FluentValidation en todo request de escritura
3. **Usar Result Pattern** para operaciones que pueden fallar
4. **Un servicio por agregado** de dominio
5. **Mantener servicios delgados**, delegar persistencia al Unit of Work
6. **No incluir lógica de infraestructura** (EF Core, SQL) en servicios

---

## 🎓 Conceptos Aplicados

- **Use Case Driven Design**
- **DTO Pattern** (Martin Fowler)
- **Result Pattern** / Railway-Oriented Programming (Scott Wlaschin)
- **FluentValidation**
- **Object-Object Mapping** (AutoMapper)
- **Dependency Injection**

---

## 📌 Notas Importantes

- Esta capa **NO debe** tener lógica de infraestructura (EF Core, SQL, etc.)
- Los servicios **coordinan**, no implementan lógica de persistencia
- Result Pattern **elimina** el uso de excepciones para control de flujo
- FluentValidation permite **validaciones complejas y reusables**
- AutoMapper **reduce boilerplate** de mapeo manual entre capas
