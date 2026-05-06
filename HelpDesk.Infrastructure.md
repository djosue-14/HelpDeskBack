# HelpDesk.Infrastructure

## 📋 Descripción General

**HelpDesk.Infrastructure** es la capa de infraestructura del proyecto HelpDesk, responsable de implementar los detalles técnicos de acceso a datos y persistencia del sistema de gestión de tickets. Esta capa implementa las interfaces definidas en la capa de dominio utilizando Entity Framework Core y SQL Server.

---

## 🎯 Propósito

Este proyecto proporciona:
- **Implementación de repositorios**: Acceso a datos concreto para todas las entidades de tickets
- **DbContext de Entity Framework**: Configuración de la base de datos HelpDesk
- **Configuraciones Fluent API**: Mapeo de entidades a tablas sin contaminar el dominio
- **Unit of Work**: Gestión de transacciones coordinadas
- **Migraciones de base de datos**: Control de cambios de esquema

---

## 🏗️ Arquitectura

### Patrón Arquitectónico
- **Clean Architecture** — Capa de infraestructura
- **Repository Pattern**: Abstracción de acceso a datos
- **Unit of Work Pattern**: Transacciones coordinadas
- **Fluent API**: Configuración de EF Core sin anotaciones en entidades

### Principios Aplicados
- ✅ **Dependency Inversion**: Implementa interfaces definidas en HelpDesk.Domain
- ✅ **Separation of Concerns**: Solo responsable de persistencia
- ✅ **Configuration over Convention**: Fluent API explícita por entidad
- ✅ **Transaction Management**: Unit of Work para consistencia

---

## 📦 Dependencias

### Proyectos Referenciados
```
HelpDesk.Infrastructure
    └── HelpDesk.Domain (Implementa interfaces de esta capa)
```

### Paquetes NuGet

| Paquete | Versión | Propósito |
|---------|---------|-----------|
| `Microsoft.EntityFrameworkCore` | 10.0.0 | ORM principal |
| `Microsoft.EntityFrameworkCore.SqlServer` | 10.0.0 | Provider de SQL Server |
| `Microsoft.EntityFrameworkCore.Design` | 10.0.0 | Herramientas de diseño |
| `Microsoft.EntityFrameworkCore.Tools` | 10.0.0 | Migraciones y scaffolding |
| `System.Security.Cryptography.Xml` | 10.0.7 | Override explícito de dependencia transitiva vulnerable (9.0.0) |

### Framework
- **.NET 10.0**
- **Nullable Reference Types**: Habilitado
- **Implicit Usings**: Habilitado

---

## 📁 Estructura del Proyecto

```
HelpDesk.Infrastructure/
├── Persistence/
│   ├── Contexts/
│   │   └── HelpDeskDbContext.cs                  # DbContext de EF Core
│   │
│   └── Configurations/                            # 10 configuraciones Fluent API
│       ├── TicketConfiguration.cs
│       ├── TicketCommentConfiguration.cs
│       ├── TicketAttachmentConfiguration.cs
│       ├── TicketHistoryConfiguration.cs
│       ├── DepartmentConfiguration.cs
│       ├── SupportTypeConfiguration.cs
│       ├── SupportTypeAgentConfiguration.cs
│       ├── SlaConfigurationConfiguration.cs
│       ├── ScoreTransactionConfiguration.cs
│       └── UserScoreConfiguration.cs
│
└── Repositories/                                  # Implementaciones de repositorios
    ├── Repository.cs                              # Repositorio genérico base
    ├── UnitOfWork.cs                              # Patrón Unit of Work
    ├── TicketRepository.cs
    ├── TicketCommentRepository.cs
    ├── TicketAttachmentRepository.cs
    ├── TicketHistoryRepository.cs
    ├── DepartmentRepository.cs
    ├── SupportTypeRepository.cs
    ├── SupportTypeAgentRepository.cs
    ├── SlaConfigurationRepository.cs
    ├── ScoreTransactionRepository.cs
    └── UserScoreRepository.cs
```

---

## 🔑 Componentes Clave

### 1. DbContext

#### **HelpDeskDbContext**
```csharp
public class HelpDeskDbContext : DbContext
{
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<TicketComment> TicketComments { get; set; }
    public DbSet<TicketAttachment> TicketAttachments { get; set; }
    public DbSet<TicketHistory> TicketHistories { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<SupportType> SupportTypes { get; set; }
    public DbSet<SupportTypeAgent> SupportTypeAgents { get; set; }
    public DbSet<SlaConfiguration> SlaConfigurations { get; set; }
    public DbSet<ScoreTransaction> ScoreTransactions { get; set; }
    public DbSet<UserScore> UserScores { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new TicketConfiguration());
        modelBuilder.ApplyConfiguration(new TicketCommentConfiguration());
        modelBuilder.ApplyConfiguration(new TicketAttachmentConfiguration());
        modelBuilder.ApplyConfiguration(new TicketHistoryConfiguration());
        modelBuilder.ApplyConfiguration(new DepartmentConfiguration());
        modelBuilder.ApplyConfiguration(new SupportTypeConfiguration());
        modelBuilder.ApplyConfiguration(new SupportTypeAgentConfiguration());
        modelBuilder.ApplyConfiguration(new SlaConfigurationConfiguration());
        modelBuilder.ApplyConfiguration(new ScoreTransactionConfiguration());
        modelBuilder.ApplyConfiguration(new UserScoreConfiguration());
    }
}
```

**Características:**
- ✅ Usa entidades de **HelpDesk.Domain**
- ✅ Configuraciones separadas en archivos individuales
- ✅ Sin configuración en entidades (sin Data Annotations)
- ✅ Fluent API para control total

---

### 2. Configuraciones Fluent API

#### **TicketConfiguration** (ejemplo)
```csharp
public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.HasKey(t => t.TicketId);
        builder.Property(t => t.TicketId).ValueGeneratedOnAdd();  // INT IDENTITY

        builder.Property(t => t.TicketNumber).IsRequired();
        builder.HasIndex(t => t.TicketNumber).IsUnique();

        builder.Property(t => t.Subject).IsRequired().HasMaxLength(200);
        builder.Property(t => t.Description).IsRequired();

        // Enums almacenados como TINYINT (no como string)
        builder.Property(t => t.Priority).IsRequired().HasColumnType("tinyint");
        builder.Property(t => t.Status).IsRequired().HasColumnType("tinyint");
        builder.Property(t => t.ResolutionCategory).HasColumnType("tinyint");

        builder.Property(t => t.AssignedUserId).HasMaxLength(25);
        builder.Property(t => t.CreatedBy).IsRequired().HasMaxLength(25);
        builder.Property(t => t.TotalPausedMinutes).IsRequired().HasDefaultValue(0);

        builder.HasIndex(t => new { t.Status, t.Priority });
        builder.HasIndex(t => t.Deadline);

        // Relaciones con navegación inversa explícita (evita shadow properties)
        builder.HasOne(t => t.Department)
            .WithMany(d => d.Tickets)
            .HasForeignKey(t => t.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.SupportType)
            .WithMany(st => st.Tickets)
            .HasForeignKey(t => t.SupportTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(t => t.Comments)
            .WithOne(c => c.Ticket)
            .HasForeignKey(c => c.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Attachments)
            .WithOne(a => a.Ticket)
            .HasForeignKey(a => a.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.History)
            .WithOne(h => h.Ticket)
            .HasForeignKey(h => h.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable("Ticket");
    }
}
```

**Ventajas de Fluent API:**
- 🎯 **Separación**: Configuración fuera de entidades
- 🔧 **Control total**: Más opciones que Data Annotations
- 📝 **Legibilidad**: Configuración centralizada por entidad
- 🧪 **Testeable**: Fácil de validar

---

#### **SupportTypeAgentConfiguration**
```csharp
public class SupportTypeAgentConfiguration : IEntityTypeConfiguration<SupportTypeAgent>
{
    public void Configure(EntityTypeBuilder<SupportTypeAgent> builder)
    {
        builder.HasKey(e => e.SupportTypeAgentId);
        builder.Property(e => e.SupportTypeAgentId).ValueGeneratedOnAdd();  // INT IDENTITY

        // UserId viene del token JWT — NVARCHAR(25), sin FK a tabla de usuarios
        builder.Property(e => e.UserId)
            .IsRequired()
            .HasMaxLength(25);

        // Garantiza un único agente activo por tipo de soporte a nivel de BD
        builder.HasIndex(e => new { e.SupportTypeId, e.IsEnabled })
            .HasFilter("([IsEnabled]=(1))")
            .IsUnique()
            .HasDatabaseName("UIX_SupportTypeAgent_SupportTypeId_Active");

        // Índice para consultar asignaciones por usuario
        builder.HasIndex(e => new { e.UserId, e.IsEnabled })
            .HasDatabaseName("IX_SupportTypeAgent_UserId_IsEnabled");

        // Relación con SupportType
        builder.HasOne(e => e.SupportType)
            .WithMany(st => st.SupportTypeAgents)
            .HasForeignKey(e => e.SupportTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable("SupportTypeAgent");
    }
}
```

**Reglas de negocio reflejadas en la configuración:**
- El índice único filtrado `UIX_SupportTypeAgents_SupportTypeId_Active` garantiza que solo exista **un registro activo por tipo de soporte** a nivel de base de datos
- El soft delete (`IsEnabled = false`) preserva el historial completo de asignaciones anteriores para auditoría
- `UserId` se almacena como cadena sin FK a una tabla de usuarios, ya que el dato proviene del token de autenticación

---

### 3. Repositorio Genérico

#### **Repository\<T\>**
```csharp
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly HelpDeskDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(HelpDeskDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }

    // ... más métodos CRUD
}
```

**Operaciones disponibles:**
- `GetByIdAsync`, `GetAllAsync`
- `FindAsync` (con expresiones lambda)
- `AddAsync`, `Update`, `Remove`
- `AnyAsync`, `CountAsync`

---

### 4. Repositorios Específicos

#### **TicketRepository** (ejemplo)
```csharp
public class TicketRepository : Repository<Ticket>, ITicketRepository
{
    public TicketRepository(HelpDeskDbContext context) : base(context) { }

    public async Task<Ticket?> GetByTicketNumberAsync(int ticketNumber, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(t => t.TicketNumber == ticketNumber && t.IsEnabled, cancellationToken);
    }

    public async Task<Ticket?> GetWithFullDetailAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.Department)
            .Include(t => t.SupportType)
            .Include(t => t.Comments.OrderBy(c => c.CreatedAt))
                .ThenInclude(c => c.Attachments)
            .Include(t => t.Attachments)
            .Include(t => t.History.OrderByDescending(h => h.CreatedAt))
            .FirstOrDefaultAsync(t => t.TicketId == id, cancellationToken);
    }

    public async Task<IEnumerable<Ticket>> GetByAgentAsync(
        string agentUserId,
        TicketStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Where(t => t.AssignedUserId == agentUserId && t.IsEnabled);

        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);

        return await query
            .OrderBy(t => t.Deadline)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Ticket>> GetOverdueSlaAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _dbSet
            .Where(t => t.IsEnabled
                     && t.Status != TicketStatus.Closed
                     && t.Status != TicketStatus.WaitingForInfo
                     && t.Deadline < now)
            .ToListAsync(cancellationToken);
    }
}
```

**Características:**
- ✅ Hereda de `Repository<T>` (operaciones base)
- ✅ Agrega métodos específicos del dominio de tickets
- ✅ Usa LINQ para queries complejas con filtros de estado y SLA
- ✅ Eager loading con `Include` solo cuando es necesario

---

### 5. Unit of Work

#### **UnitOfWork**
```csharp
public class UnitOfWork : IUnitOfWork
{
    private readonly HelpDeskDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(HelpDeskDbContext context)
    {
        _context = context;

        // Inicializa todos los repositorios
        Tickets = new TicketRepository(_context);
        TicketComments = new TicketCommentRepository(_context);
        TicketAttachments = new TicketAttachmentRepository(_context);
        TicketHistories = new TicketHistoryRepository(_context);
        Departments = new DepartmentRepository(_context);
        SupportTypes = new SupportTypeRepository(_context);
        SupportTypeAgents = new SupportTypeAgentRepository(_context);
        SlaConfigurations = new SlaConfigurationRepository(_context);
        ScoreTransactions = new ScoreTransactionRepository(_context);
        UserScores = new UserScoreRepository(_context);
    }

    // Propiedades de repositorios
    public ITicketRepository Tickets { get; }
    public ITicketCommentRepository TicketComments { get; }
    public ITicketAttachmentRepository TicketAttachments { get; }
    public ITicketHistoryRepository TicketHistories { get; }
    public IDepartmentRepository Departments { get; }
    public ISupportTypeRepository SupportTypes { get; }
    public ISupportTypeAgentRepository SupportTypeAgents { get; }
    public ISlaConfigurationRepository SlaConfigurations { get; }
    public IScoreTransactionRepository ScoreTransactions { get; }
    public IUserScoreRepository UserScores { get; }

    // Gestión de transacciones
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        await _transaction?.CommitAsync();
        await _transaction?.DisposeAsync();
    }

    public async Task RollbackTransactionAsync()
    {
        await _transaction?.RollbackAsync();
        await _transaction?.DisposeAsync();
    }
}
```

**Ventajas:**
- 🎯 **Un SaveChanges para todos**: Consistencia transaccional
- 🔒 **Transacciones explícitas**: Control total del ciclo
- 📦 **Acceso centralizado**: Un punto de entrada a todos los repositorios
- 🔄 **Atomic operations**: Todo o nada

---

## 🔄 Flujo de Datos

```
Service Layer (HelpDesk.Application)
    ↓ Llama a IUnitOfWork
UnitOfWork
    ↓ Accede al repositorio específico
Repository
    ↓ Usa HelpDeskDbContext
Entity Framework Core
    ↓ Genera SQL
SQL Server Database
```

---

## 🎨 Patrones Utilizados

### 1. **Repository Pattern**
- Abstracción del acceso a datos
- Interfaz definida en Domain, implementación en Infrastructure

### 2. **Unit of Work Pattern**
- Gestión de transacciones coordinadas
- Coordinación de múltiples repositorios en un solo `SaveChanges`

### 3. **Fluent API Configuration**
- Configuración separada de las entidades de dominio
- Mapeo explícito ORM sin contaminar el modelo

### 4. **Generic Repository**
- Operaciones CRUD comunes heredables
- Extensión mediante repositorios específicos por entidad

---

## 🗄️ Base de Datos

### Conexión (appsettings.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=HelpDesk;User Id=SA;Password=***;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

### Tablas Principales

| Tabla | Descripción | Índices Principales |
|-------|-------------|---------------------|
| `Ticket` | Tickets de soporte | TicketNumber (UK), Status+Priority (IX), Deadline (IX) |
| `TicketComment` | Hilo de comunicación | TicketId (FK), CommentId (FK opcional) |
| `TicketAttachment` | Archivos adjuntos | TicketId (FK), CommentId (FK opcional, SET NULL) |
| `TicketHistory` | Auditoría de acciones | TicketId (FK) |
| `Department` | Departamentos | Name (UK) |
| `SupportType` | Tipos de soporte | DepartmentId+Name (UK) |
| `SupportTypeAgent` | Asignación agente ↔ tipo de soporte | SupportTypeId+IsEnabled (UIX filtrado), UserId+IsEnabled (IX) |
| `SlaConfiguration` | Tiempos SLA por prioridad | Priority (UK) |
| `ScoreTransaction` | Transacciones de puntos | UserId+CreatedAt (IX) |
| `UserScore` | Perfil de reputación | UserId (UK) |

### Características de BD
- ✅ **Soft Delete**: Campo `IsEnabled` en lugar de DELETE físico
- ✅ **Auditoría**: `CreatedAt`, `CreatedBy`, `LastModifiedAt`, `LastModifiedBy`
- ✅ **SLA Tracking**: `Deadline`, `TotalPausedMinutes` en tickets
- ✅ **Enum as Tinyint**: Enums almacenados como `TINYINT` (más eficiente que string)
- ✅ **Índices optimizados**: Para consultas frecuentes de bandeja y SLA
- ✅ **PKs INT IDENTITY**: Enteros autoincrement (no GUIDs) para mejor rendimiento de índices

---

## ✨ Características Destacadas

### Almacenamiento de Enums como Tinyint
```csharp
builder.Property(t => t.Status)
    .IsRequired()
    .HasColumnType("tinyint");

builder.Property(t => t.Priority)
    .IsRequired()
    .HasColumnType("tinyint");
```

### PKs con INT IDENTITY (ValueGeneratedOnAdd)
```csharp
builder.HasKey(t => t.TicketId);
builder.Property(t => t.TicketId).ValueGeneratedOnAdd();
```

### Índices Filtrados (un agente activo por tipo de soporte)
```csharp
builder.HasIndex(e => new { e.SupportTypeId, e.IsEnabled })
    .HasFilter("([IsEnabled]=(1))")
    .IsUnique()
    .HasDatabaseName("UIX_SupportTypeAgent_SupportTypeId_Active");
```

### Eager Loading de Ticket Completo
```csharp
return await _dbSet
    .Include(t => t.Department)
    .Include(t => t.SupportType)
    .Include(t => t.Comments.OrderBy(c => c.CreatedAt))
        .ThenInclude(c => c.Attachments)
    .Include(t => t.History.OrderByDescending(h => h.CreatedAt))
    .FirstOrDefaultAsync(t => t.TicketId == id, cancellationToken);
```

### Relación Opcional con SetNull
```csharp
// TicketAttachment.CommentId es nullable: al eliminar el comentario, el adjunto se preserva
builder.HasMany(c => c.Attachments)
    .WithOne(a => a.Comment)
    .HasForeignKey(a => a.CommentId)
    .IsRequired(false)
    .OnDelete(DeleteBehavior.SetNull);
```

---

## 🔗 Proyectos que Dependen de Este

- **HelpDesk.API** ← Registra el DbContext e inyecta el UnitOfWork

---

## 🔗 Dependencias de Este Proyecto

- **HelpDesk.Domain** ← Implementa interfaces y usa entidades

---

## 📝 Convenciones

### Naming
- **Tablas**: PascalCase singular (ej: `Ticket`, `SupportType`) — definido con `builder.ToTable("Ticket")`
- **Columnas**: PascalCase (ej: `TicketId`, `AssignedUserId`)
- **Constraints**: `PK_`, `FK_`, `UK_`, `IX_` como prefijos
- **Índices**: `IX_{Table}_{Column(s)}`

### Organización
- **Una configuración por entidad**
- **Repositorio genérico + específicos**
- **Un DbContext para toda la aplicación**

---

## 🚀 Uso

### Configuración en Program.cs
```csharp
// DbContext
builder.Services.AddDbContext<HelpDeskDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()));

// Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
```

### Uso en Servicios
```csharp
public class TicketService : ITicketService
{
    private readonly IUnitOfWork _unitOfWork;

    public TicketService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<TicketDto>> CreateAsync(CreateTicketRequest request, string createdBy, CancellationToken cancellationToken)
    {
        var ticket = new Ticket { ... };
        await _unitOfWork.Tickets.AddAsync(ticket, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<TicketDto>.Success(dto);
    }
}
```

### Transacciones Explícitas
```csharp
await _unitOfWork.BeginTransactionAsync();
try
{
    await _unitOfWork.Tickets.AddAsync(ticket, cancellationToken);
    await _unitOfWork.TicketHistories.AddAsync(historyEntry, cancellationToken);
    await _unitOfWork.SaveChangesAsync(cancellationToken);

    await _unitOfWork.CommitTransactionAsync();
}
catch
{
    await _unitOfWork.RollbackTransactionAsync();
    throw;
}
```

---

## 🛠️ Comandos EF Core

### Crear Migración
```bash
dotnet ef migrations add NombreMigracion \
  --startup-project ../HelpDesk.API \
  --project ../HelpDesk.Infrastructure
```

### Aplicar Migraciones
```bash
dotnet ef database update \
  --startup-project ../HelpDesk.API \
  --project ../HelpDesk.Infrastructure
```

### Scaffold desde BD (uso inicial)
```bash
dotnet ef dbcontext scaffold "Name=ConnectionStrings:HelpDeskDb" \
  Microsoft.EntityFrameworkCore.SqlServer \
  --startup-project ./HelpDesk.API \
  --project ./HelpDesk.Infrastructure \
  --output-dir Persistence/Entities \
  --context-dir Persistence/Contexts \
  --context HelpDeskDbContext \
  --data-annotations \
  --force
```

---

## 💡 Mejores Prácticas

1. **Usar Fluent API** en lugar de Data Annotations
2. **Separar configuraciones** en archivos individuales por entidad
3. **Usar Unit of Work** para operaciones relacionadas
4. **Eager load con Include** solo cuando sea estrictamente necesario
5. **No exponer DbContext** fuera de Infrastructure
6. **Usar transacciones** para operaciones críticas del ticket
7. **Implementar retry policies** para resiliencia ante fallos de red

---

## 🎓 Conceptos Aplicados

- **Repository Pattern** (Martin Fowler)
- **Unit of Work Pattern** (Martin Fowler)
- **ORM (Object-Relational Mapping)**
- **Entity Framework Core**
- **Fluent API Configuration**
- **Dependency Injection**
- **Transaction Management**

---

## 📌 Notas Importantes

- Las entidades **pertenecen a HelpDesk.Domain**, no a Infrastructure
- Fluent API **mantiene entidades limpias** sin anotaciones
- Unit of Work **garantiza consistencia** transaccional en operaciones de tickets
- Los repositorios **abstraen** la tecnología de persistencia
- Esta capa es **sustituible** (se podría cambiar a Dapper, MongoDB, etc.)
