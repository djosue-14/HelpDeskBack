# HelpDesk.Domain

## 📋 Descripción General

**HelpDesk.Domain** es la capa de dominio del proyecto HelpDesk, el corazón de la arquitectura Clean Architecture. Este proyecto contiene la lógica de negocio pura y las reglas fundamentales del dominio de gestión de tickets de soporte, completamente independiente de cualquier tecnología o framework externo.

---

## 🎯 Propósito

Este proyecto define:
- **Entidades de dominio**: Modelos de negocio puros sin dependencias de infraestructura
- **Interfaces de repositorio**: Contratos que definen cómo se accede a los datos
- **Enumeraciones**: Tipos de datos específicos del dominio (estados, prioridades, roles)
- **Value Objects**: Objetos de valor inmutables (preparado para expansión)
- **Reglas de negocio**: Lógica fundamental del sistema de tickets

---

## 🏗️ Arquitectura

### Patrón Arquitectónico
- **Clean Architecture** / **Domain-Driven Design (DDD)**
- **Capa más interna**: No depende de ningún otro proyecto
- **Independencia tecnológica**: Sin referencias a EF Core, ASP.NET, etc.

### Principios Aplicados
- ✅ **Separation of Concerns**: Lógica de negocio separada de infraestructura
- ✅ **Dependency Inversion**: Define interfaces, no implementaciones
- ✅ **Single Responsibility**: Cada entidad tiene una responsabilidad clara
- ✅ **Domain-Centric**: El dominio es el núcleo del sistema

---

## 📦 Dependencias

### Proyectos Referenciados
**Ninguno** — Este proyecto no depende de otros proyectos (es la capa más interna)

### Paquetes NuGet
**Ninguno** — Solo usa librerías base de .NET 10.0

### Framework
- **.NET 10.0**
- **Nullable Reference Types**: Habilitado
- **Implicit Usings**: Habilitado

---

## 📁 Estructura del Proyecto

```
HelpDesk.Domain/
├── Common/
│   └── BaseAuditableEntity.cs              # Clase base para entidades con auditoría
│
├── Entities/                                # 10 entidades de dominio
│   ├── Ticket.cs                            # Ticket de soporte (entidad raíz)
│   ├── TicketComment.cs                     # Comentarios del hilo del ticket
│   ├── TicketAttachment.cs                  # Archivos adjuntos del ticket
│   ├── TicketHistory.cs                     # Registro de auditoría del ciclo de vida
│   ├── Department.cs                        # Departamentos configurables
│   ├── SupportType.cs                       # Tipos de soporte por departamento
│   ├── SupportTypeAgent.cs                  # Tabla intermedia: agente asignado a tipo de soporte
│   ├── SlaConfiguration.cs                  # Configuración de tiempos SLA por prioridad
│   ├── ScoreTransaction.cs             # Transacciones del sistema de puntos
│   └── UserScore.cs                    # Perfil de reputación del solicitante
│
├── Enums/                                   # 7 enumeraciones del dominio
│   ├── TicketStatus.cs                      # Estados del ticket
│   ├── TicketPriority.cs                    # Niveles de prioridad
│   ├── ResolutionCategory.cs                # Categorías de resolución al cierre
│   ├── UserRole.cs                          # Roles del sistema
│   ├── ScoreLevel.cs                   # Niveles de reputación del solicitante
│   ├── ScoreTransactionReason.cs       # Motivos de puntos ganados/perdidos
│   └── CommentVisibility.cs                 # Visibilidad del comentario (público / interno)
│
├── Interfaces/                              # Contratos de repositorio
│   ├── IRepository.cs                       # Repositorio genérico
│   ├── IUnitOfWork.cs                       # Patrón Unit of Work
│   ├── ITicketRepository.cs
│   ├── ITicketCommentRepository.cs
│   ├── ITicketAttachmentRepository.cs
│   ├── ITicketHistoryRepository.cs
│   ├── IDepartmentRepository.cs
│   ├── ISupportTypeRepository.cs
│   ├── ISupportTypeAgentRepository.cs
│   ├── ISlaConfigurationRepository.cs
│   ├── IScoreTransactionRepository.cs
│   └── IUserScoreRepository.cs
│
└── ValueObjects/                            # (Preparado para expansión)
```

---

## 🔑 Componentes Clave

### 1. Entidades de Dominio

#### **BaseAuditableEntity**
Clase base abstracta que proporciona campos de auditoría comunes:
- `CreatedAt`, `CreatedBy`
- `LastModifiedAt`, `LastModifiedBy`
- `IsEnabled`, `DisabledAt`, `DisabledBy`

#### **Ticket**
Entidad raíz del sistema. Representa una solicitud de soporte.
- Identificador único y número de ticket correlativo
- Departamento y tipo de soporte asociados
- Prioridad, estado actual y fechas de ciclo de vida (solicitud, primera apertura, inicio de trabajo, cierre)
- Fecha límite calculada según configuración SLA y acumulado de pausas
- Agente asignado, categoría de resolución y referencia al ticket de origen (tickets relacionados)

#### **TicketComment**
Representa un mensaje dentro del hilo de comunicación del ticket.
- Contenido, autor y marca de tiempo
- Visibilidad: `Public` (visible para el solicitante) o `Internal` (solo equipo de soporte)
- Referencia al ticket padre

#### **TicketAttachment**
Archivo adjunto vinculado a un ticket o a un comentario específico.
- Nombre original, extensión, tamaño en bytes y ruta de almacenamiento
- Referencia al ticket y al comentario si aplica
- Usuario que realizó la carga y fecha

#### **TicketHistory**
Registro inmutable de auditoría de cada acción realizada sobre el ticket.
- Tipo de acción (ej. `StatusChanged`, `CommentAdded`, `Reassigned`)
- Valor anterior y valor nuevo del campo modificado
- Usuario ejecutor (o `"Sistema"` para acciones automáticas)
- Marca de tiempo exacta con precisión de segundos

#### **Department**
Departamento organizacional al que pertenecen los tipos de soporte.
- Nombre, descripción e identificador del coordinador responsable
- Relación con los tipos de soporte y los tickets asociados

#### **SupportType**
Categoría de soporte dentro de un departamento.
- Nombre y descripción
- Relación con el departamento al que pertenece
- El agente responsable **no se almacena directamente aquí** — se gestiona mediante `SupportTypeAgent` para mayor trazabilidad y soporte de soft delete

#### **SupportTypeAgent**
Tabla intermedia que vincula un tipo de soporte con el usuario responsable de atenderlo. El `UserId` proviene del token de autenticación en el momento del registro o reasignación.
- `SupportTypeAgentId` — identificador único del registro
- `SupportTypeId` — referencia al tipo de soporte
- `UserId` — identificador del agente extraído del token (sin almacenar datos del usuario)
- Hereda `BaseAuditableEntity`: `CreatedAt`, `CreatedBy`, `IsEnabled`, `DisabledAt`, `DisabledBy`
- Solo el registro con `IsEnabled = true` representa la asignación activa
- El historial de asignaciones anteriores se preserva con `IsEnabled = false` (soft delete)
- Permite cambiar el agente responsable sin perder trazabilidad

#### **SlaConfiguration**
Configuración del tiempo límite de resolución por nivel de prioridad.
- Nivel de prioridad y horas límite correspondientes (solo enteros positivos)
- Aplicación prospectiva: los tickets existentes no se ven afectados

#### **ScoreTransaction**
Registro individual de puntos ganados o perdidos por el solicitante.
- Puntos (valor positivo o negativo), motivo y fecha de la transacción
- Referencia al ticket que originó el evento

#### **UserScore**
Perfil de reputación acumulado del solicitante.
- Saldo actual de puntos (mínimo: 0), nivel de reputación activo
- Referencia al historial de transacciones individuales

---

### 2. Enumeraciones

#### **TicketStatus**
```csharp
Open, InProgress, WaitingForInfo, Closed, Reopened
```

#### **TicketPriority**
```csharp
Critical, High, Medium, Low
```

#### **ResolutionCategory**
```csharp
Resolved, Rejected, Duplicate, ClosedNoResponse
```

#### **UserRole**
```csharp
Requester, SupportAgent, Coordinator, Administrator, AutomatedSystem
```

#### **ScoreLevel**
```csharp
Bronze, Silver, Gold
```

#### **ScoreTransactionReason**
```csharp
RatingSubmitted, RatingWithComment, PenaltyRejected,
PenaltyDuplicate, PenaltyRedirected, LevelUpgrade, LevelDowngrade
```

#### **CommentVisibility**
```csharp
Public, Internal
```

---

### 3. Interfaces de Repositorio

#### **IRepository\<T\>**
Repositorio genérico con operaciones CRUD:
- `GetByIdAsync`, `GetAllAsync`
- `FindAsync`, `FirstOrDefaultAsync`
- `AddAsync`, `Update`, `Remove`
- `AnyAsync`, `CountAsync`

#### **IUnitOfWork**
Patrón Unit of Work para transacciones:
- Acceso a todos los repositorios específicos
- `SaveChangesAsync`
- `BeginTransactionAsync`, `CommitTransactionAsync`, `RollbackTransactionAsync`

---

## 🔄 Relaciones Entre Entidades

```
Department       (1) ──→ (*) SupportType
                 (1) ──→ (*) Ticket

SupportType      (1) ──→ (*) SupportTypeAgent   [tabla intermedia de asignación]
                 (1) ──→ (*) Ticket

SupportTypeAgent (*) ──→ (1) SupportType        [SupportTypeId]
                          UserId                 [viene del token, sin FK a tabla usuarios]
                          IsEnabled = true       [solo un registro activo por SupportType]

SlaConfiguration (1) ──→ (1) TicketPriority     [configuración global]

Ticket           (1) ──→ (*) TicketComment
                 (1) ──→ (*) TicketAttachment
                 (1) ──→ (*) TicketHistory
                 (1) ──→ (0..1) Ticket           [ticket relacionado origen]

TicketComment    (1) ──→ (*) TicketAttachment

UserScore   (1) ──→ (*) ScoreTransaction
```

---

## ✨ Características Destacadas

### Independencia Total
- ❌ **No tiene** referencias a Entity Framework
- ❌ **No tiene** referencias a ASP.NET
- ❌ **No tiene** anotaciones de infraestructura
- ✅ **Solo** lógica de negocio pura

### Diseño Limpio
- **Entidades POCO** (Plain Old CLR Objects)
- **Interfaces en lugar de implementaciones**
- **Enumeraciones fuertemente tipadas**
- **Separation of Concerns**

### Extensibilidad
- Preparado para agregar Value Objects (ej. `SlaDeadline`, `TicketNumber`)
- Preparado para agregar Domain Events (ej. `TicketClosedEvent`, `SlaBreachedEvent`)
- Preparado para agregar Domain Services

---

## 🔗 Proyectos que Dependen de Este

- **HelpDesk.Application** ← Usa las entidades e interfaces
- **HelpDesk.Infrastructure** ← Implementa las interfaces

---

## 📝 Convenciones

### Naming
- **Entidades**: PascalCase (ej: `Ticket`, `SupportType`)
- **Propiedades**: PascalCase (ej: `TicketId`, `ResolutionCategory`)
- **Interfaces**: Prefijo `I` + PascalCase (ej: `ITicketRepository`)

### Nullable Reference Types
- **Habilitado**: Todas las referencias nullable están explícitamente marcadas
- **Non-nullable por defecto**: Mejora la seguridad del código

### Mapeo a Base de Datos (SQL Server 2016+)
- **PKs (`*Id`)**: `INT IDENTITY(1,1)` — entero autoincrementable, sin GUIDs
- **FKs**: `INT NOT NULL` (o `INT NULL` si la relación es opcional)
- **TicketNumber**: `INT NOT NULL` — número correlativo asignado por la capa de aplicación (no IDENTITY)
- **Enums**: `TINYINT` con `CHECK CONSTRAINT` según valores del dominio
- **Campos de usuario/auditoría** (`CreatedBy`, `LastModifiedBy`, `DisabledBy`, `UserId`, `AuthorId`, `AssignedUserId`, etc.): `NVARCHAR(25)`
- **Textos cortos**: `NVARCHAR(100)`–`NVARCHAR(500)`
- **Textos largos** (`Description`, `Content`, `PreviousValue`, `NewValue`): `NVARCHAR(MAX)`
- **Fechas**: `DATETIME2(7)` (alta precisión) / `DATETIME2(0)` para `ExecutedAt` (precisión de segundos)
- **Auditoría de baja** (`DisabledAt`, `DisabledBy`): obligatorios cuando `IsEnabled = 0` (enforced por CHECK constraint)

---

## 🚀 Uso

Este proyecto **NO se ejecuta directamente**. Es una biblioteca de clases que:
1. Define el modelo de dominio de gestión de tickets de soporte
2. Establece contratos (interfaces) de acceso a datos
3. Es referenciado por las capas de aplicación e infraestructura

---

## 💡 Mejores Prácticas

1. **No agregar dependencias externas** a este proyecto
2. **Mantener entidades limpias** sin lógica de infraestructura
3. **Usar Value Objects** para conceptos del dominio (ej: `SlaDeadline`, `TicketNumber`)
4. **Agregar Domain Events** cuando sea necesario comunicar cambios entre agregados
5. **No exponer setters públicos** innecesariamente

---

## 🎓 Conceptos Aplicados

- **Clean Architecture** (Uncle Bob)
- **Domain-Driven Design** (Eric Evans)
- **Repository Pattern**
- **Unit of Work Pattern**
- **SOLID Principles**

---

## 📌 Notas Importantes

- Este proyecto es el **núcleo** de toda la aplicación HelpDesk
- Cualquier cambio aquí **afecta** a todo el sistema
- Mantener la **independencia** de frameworks externos es crítico
- Las entidades son **agnósticas** a la persistencia
