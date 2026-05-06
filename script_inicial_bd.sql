-- ============================================================
-- HelpDesk Database - Script Inicial
-- SQL Server 2016+ (compatible con versión 13.x en adelante)
-- Convención: PascalCase para tablas y columnas
-- PKs: INT IDENTITY(1,1)
-- Enums almacenados como TINYINT con CHECK constraints
-- Campos de usuario/auditoría: NVARCHAR(25)
-- ============================================================

USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'HelpDesk')
BEGIN
    CREATE DATABASE HelpDesk
        COLLATE SQL_Latin1_General_CP1_CI_AS;
END
GO

USE HelpDesk;
GO

-- ============================================================
-- ENUMERACIONES DE REFERENCIA (comentarios documentales)
-- TicketPriority    : 0=Critical  | 1=High       | 2=Medium   | 3=Low
-- TicketStatus      : 0=Open      | 1=InProgress | 2=WaitingForInfo | 3=Closed | 4=Reopened
-- ResolutionCategory: 0=Resolved  | 1=Rejected   | 2=Duplicate | 3=ClosedNoResponse
-- CommentVisibility : 0=Public    | 1=Internal
-- ScoreLevel        : 0=Bronze    | 1=Silver     | 2=Gold
-- ScoreTransactionReason:
--   0=RatingSubmitted | 1=RatingWithComment | 2=PenaltyRejected
--   3=PenaltyDuplicate | 4=PenaltyRedirected | 5=LevelUpgrade | 6=LevelDowngrade
-- ============================================================

-- ============================================================
-- TABLA: Department
-- ============================================================
CREATE TABLE Department (
    DepartmentId        INT             NOT NULL    IDENTITY(1, 1),
    Name                NVARCHAR(100)   NOT NULL,
    Description         NVARCHAR(500)   NULL,
    CoordinatorUserId   NVARCHAR(25)    NULL,
    -- Auditoría (BaseAuditableEntity)
    CreatedAt           DATETIME2(7)    NOT NULL    DEFAULT SYSUTCDATETIME(),
    CreatedBy           NVARCHAR(25)    NOT NULL,
    LastModifiedAt      DATETIME2(7)    NULL,
    LastModifiedBy      NVARCHAR(25)    NULL,
    IsEnabled           BIT             NOT NULL    DEFAULT 1,
    DisabledAt          DATETIME2(7)    NULL,
    DisabledBy          NVARCHAR(25)    NULL,

    CONSTRAINT PK_Department PRIMARY KEY (DepartmentId),
    CONSTRAINT CK_Department_DisabledConsistency
        CHECK (IsEnabled = 1 OR (IsEnabled = 0 AND DisabledAt IS NOT NULL AND DisabledBy IS NOT NULL))
);
GO

-- ============================================================
-- TABLA: SlaConfiguration
-- ============================================================
CREATE TABLE SlaConfiguration (
    SlaConfigurationId  INT             NOT NULL    IDENTITY(1, 1),
    Priority            TINYINT         NOT NULL,   -- TicketPriority enum
    HoursLimit          INT             NOT NULL,
    -- Auditoría (BaseAuditableEntity)
    CreatedAt           DATETIME2(7)    NOT NULL    DEFAULT SYSUTCDATETIME(),
    CreatedBy           NVARCHAR(25)    NOT NULL,
    LastModifiedAt      DATETIME2(7)    NULL,
    LastModifiedBy      NVARCHAR(25)    NULL,
    IsEnabled           BIT             NOT NULL    DEFAULT 1,
    DisabledAt          DATETIME2(7)    NULL,
    DisabledBy          NVARCHAR(25)    NULL,

    CONSTRAINT PK_SlaConfiguration PRIMARY KEY (SlaConfigurationId),
    CONSTRAINT UQ_SlaConfiguration_Priority UNIQUE (Priority),
    CONSTRAINT CK_SlaConfiguration_Priority
        CHECK (Priority BETWEEN 0 AND 3),
    CONSTRAINT CK_SlaConfiguration_HoursLimit
        CHECK (HoursLimit > 0),
    CONSTRAINT CK_SlaConfiguration_DisabledConsistency
        CHECK (IsEnabled = 1 OR (IsEnabled = 0 AND DisabledAt IS NOT NULL AND DisabledBy IS NOT NULL))
);
GO

-- ============================================================
-- TABLA: SupportType
-- ============================================================
CREATE TABLE SupportType (
    SupportTypeId   INT             NOT NULL    IDENTITY(1, 1),
    DepartmentId    INT             NOT NULL,
    Name            NVARCHAR(100)   NOT NULL,
    Description     NVARCHAR(500)   NULL,
    -- Auditoría (BaseAuditableEntity)
    CreatedAt       DATETIME2(7)    NOT NULL    DEFAULT SYSUTCDATETIME(),
    CreatedBy       NVARCHAR(25)    NOT NULL,
    LastModifiedAt  DATETIME2(7)    NULL,
    LastModifiedBy  NVARCHAR(25)    NULL,
    IsEnabled       BIT             NOT NULL    DEFAULT 1,
    DisabledAt      DATETIME2(7)    NULL,
    DisabledBy      NVARCHAR(25)    NULL,

    CONSTRAINT PK_SupportType PRIMARY KEY (SupportTypeId),
    CONSTRAINT FK_SupportType_Department_DepartmentId
        FOREIGN KEY (DepartmentId) REFERENCES Department (DepartmentId),
    CONSTRAINT CK_SupportType_DisabledConsistency
        CHECK (IsEnabled = 1 OR (IsEnabled = 0 AND DisabledAt IS NOT NULL AND DisabledBy IS NOT NULL))
);
GO

-- ============================================================
-- TABLA: SupportTypeAgent
-- Tabla intermedia: agente responsable de un tipo de soporte.
-- Solo IsEnabled = 1 es la asignación activa; anteriores conservan IsEnabled = 0.
-- ============================================================
CREATE TABLE SupportTypeAgent (
    SupportTypeAgentId  INT             NOT NULL    IDENTITY(1, 1),
    SupportTypeId       INT             NOT NULL,
    UserId              NVARCHAR(25)    NOT NULL,
    -- Auditoría (BaseAuditableEntity)
    CreatedAt           DATETIME2(7)    NOT NULL    DEFAULT SYSUTCDATETIME(),
    CreatedBy           NVARCHAR(25)    NOT NULL,
    LastModifiedAt      DATETIME2(7)    NULL,
    LastModifiedBy      NVARCHAR(25)    NULL,
    IsEnabled           BIT             NOT NULL    DEFAULT 1,
    DisabledAt          DATETIME2(7)    NULL,
    DisabledBy          NVARCHAR(25)    NULL,

    CONSTRAINT PK_SupportTypeAgent PRIMARY KEY (SupportTypeAgentId),
    CONSTRAINT FK_SupportTypeAgent_SupportType_SupportTypeId
        FOREIGN KEY (SupportTypeId) REFERENCES SupportType (SupportTypeId),
    CONSTRAINT CK_SupportTypeAgent_DisabledConsistency
        CHECK (IsEnabled = 1 OR (IsEnabled = 0 AND DisabledAt IS NOT NULL AND DisabledBy IS NOT NULL))
);
GO

-- ============================================================
-- TABLA: UserScore
-- Perfil de reputación acumulado del solicitante.
-- UserId con restricción UNIQUE para servir como FK desde ScoreTransaction.
-- ============================================================
CREATE TABLE UserScore (
    UserScoreId     INT             NOT NULL    IDENTITY(1, 1),
    UserId          NVARCHAR(25)    NOT NULL,
    CurrentPoints   INT             NOT NULL    DEFAULT 0,
    Level           TINYINT         NOT NULL    DEFAULT 0,  -- ScoreLevel enum

    CONSTRAINT PK_UserScore PRIMARY KEY (UserScoreId),
    CONSTRAINT UQ_UserScore_UserId UNIQUE (UserId),
    CONSTRAINT CK_UserScore_CurrentPoints
        CHECK (CurrentPoints >= 0),
    CONSTRAINT CK_UserScore_Level
        CHECK (Level BETWEEN 0 AND 2)
);
GO

-- ============================================================
-- TABLA: Ticket
-- Entidad raíz del sistema de tickets.
-- TicketId: PK INT IDENTITY(1,1).
-- TicketNumber: número correlativo asignado por la aplicación (INT NOT NULL).
-- RelatedTicketId: FK auto-referencial (ticket de origen).
-- ============================================================
CREATE TABLE Ticket (
    TicketId            INT             NOT NULL    IDENTITY(1, 1),
    TicketNumber        INT             NOT NULL,
    Subject             NVARCHAR(200)   NOT NULL,
    Description         NVARCHAR(MAX)   NOT NULL,
    DepartmentId        INT             NOT NULL,
    SupportTypeId       INT             NOT NULL,
    Priority            TINYINT         NOT NULL,   -- TicketPriority enum
    Status              TINYINT         NOT NULL    DEFAULT 0,  -- TicketStatus enum; 0=Open
    ResolutionCategory  TINYINT         NULL,       -- ResolutionCategory enum; NULL mientras no esté cerrado
    AssignedUserId      NVARCHAR(25)    NULL,
    RequestedAt         DATETIME2(7)    NOT NULL    DEFAULT SYSUTCDATETIME(),
    FirstOpenedAt       DATETIME2(7)    NULL,
    WorkStartedAt       DATETIME2(7)    NULL,
    ClosedAt            DATETIME2(7)    NULL,
    Deadline            DATETIME2(7)    NULL,
    PausedAt            DATETIME2(7)    NULL,
    TotalPausedMinutes  INT             NOT NULL    DEFAULT 0,
    CreatedAt           DATETIME2(7)    NOT NULL    DEFAULT SYSUTCDATETIME(),
    RelatedTicketId     INT             NULL,
    IsEnabled           BIT             NOT NULL    DEFAULT 1,
    CreatedBy           NVARCHAR(25)    NOT NULL,
    CONSTRAINT PK_Ticket PRIMARY KEY (TicketId),
    CONSTRAINT UQ_Ticket_TicketNumber UNIQUE (TicketNumber),
    CONSTRAINT FK_Ticket_Department_DepartmentId
        FOREIGN KEY (DepartmentId) REFERENCES Department (DepartmentId),
    CONSTRAINT FK_Ticket_SupportType_SupportTypeId
        FOREIGN KEY (SupportTypeId) REFERENCES SupportType (SupportTypeId),
    CONSTRAINT FK_Ticket_Ticket_RelatedTicketId
        FOREIGN KEY (RelatedTicketId) REFERENCES Ticket (TicketId),
    CONSTRAINT CK_Ticket_Priority
        CHECK (Priority BETWEEN 0 AND 3),
    CONSTRAINT CK_Ticket_Status
        CHECK (Status BETWEEN 0 AND 4),
    CONSTRAINT CK_Ticket_ResolutionCategory
        CHECK (ResolutionCategory IS NULL OR ResolutionCategory BETWEEN 0 AND 3),
    CONSTRAINT CK_Ticket_TotalPausedMinutes
        CHECK (TotalPausedMinutes >= 0),
    -- ResolutionCategory requerida cuando el ticket se cierra (Status = 3)
    CONSTRAINT CK_Ticket_ResolutionCategory_OnClosed
        CHECK (Status != 3 OR ResolutionCategory IS NOT NULL)
);
GO

-- ============================================================
-- TABLA: TicketComment
-- Mensajes del hilo de comunicación del ticket.
-- ============================================================
CREATE TABLE TicketComment (
    TicketCommentId INT             NOT NULL    IDENTITY(1, 1),
    TicketId        INT             NOT NULL,
    Content         NVARCHAR(MAX)   NOT NULL,
    AuthorId        NVARCHAR(25)    NOT NULL,
    Visibility      TINYINT         NOT NULL    DEFAULT 0,  -- CommentVisibility: 0=Public, 1=Internal
    CreatedAt       DATETIME2(7)    NOT NULL    DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_TicketComment PRIMARY KEY (TicketCommentId),
    CONSTRAINT FK_TicketComment_Ticket_TicketId
        FOREIGN KEY (TicketId) REFERENCES Ticket (TicketId),
    CONSTRAINT CK_TicketComment_Visibility
        CHECK (Visibility BETWEEN 0 AND 1)
);
GO

-- ============================================================
-- TABLA: TicketAttachment
-- Archivos adjuntos de un ticket o de un comentario específico.
-- CommentId es nullable: adjunto directo al ticket si es NULL.
-- ============================================================
CREATE TABLE TicketAttachment (
    TicketAttachmentId  INT             NOT NULL    IDENTITY(1, 1),
    TicketId            INT             NOT NULL,
    CommentId           INT             NULL,
    OriginalFileName    NVARCHAR(260)   NOT NULL,
    FileExtension       NVARCHAR(20)    NOT NULL,
    FileSizeBytes       BIGINT          NOT NULL,
    StoragePath         NVARCHAR(1000)  NOT NULL,
    UploadedBy          NVARCHAR(25)    NOT NULL,
    UploadedAt          DATETIME2(7)    NOT NULL    DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_TicketAttachment PRIMARY KEY (TicketAttachmentId),
    CONSTRAINT FK_TicketAttachment_Ticket_TicketId
        FOREIGN KEY (TicketId) REFERENCES Ticket (TicketId),
    CONSTRAINT FK_TicketAttachment_TicketComment_CommentId
        FOREIGN KEY (CommentId) REFERENCES TicketComment (TicketCommentId),
    CONSTRAINT CK_TicketAttachment_FileSizeBytes
        CHECK (FileSizeBytes > 0)
);
GO

-- ============================================================
-- TABLA: TicketHistory
-- Registro inmutable de auditoría del ciclo de vida del ticket.
-- ExecutedAt con precisión de segundos (DATETIME2(0)).
-- ============================================================
CREATE TABLE TicketHistory (
    TicketHistoryId INT             NOT NULL    IDENTITY(1, 1),
    TicketId        INT             NOT NULL,
    ActionType      NVARCHAR(100)   NOT NULL,
    PreviousValue   NVARCHAR(MAX)   NULL,
    NewValue        NVARCHAR(MAX)   NULL,
    ExecutedBy      NVARCHAR(25)    NOT NULL,
    ExecutedAt      DATETIME2(0)    NOT NULL    DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_TicketHistory PRIMARY KEY (TicketHistoryId),
    CONSTRAINT FK_TicketHistory_Ticket_TicketId
        FOREIGN KEY (TicketId) REFERENCES Ticket (TicketId)
);
GO

-- ============================================================
-- TABLA: ScoreTransaction
-- Registro individual de puntos ganados o perdidos.
-- UserId referencia UserScore.UserId (único por solicitante).
-- ============================================================
CREATE TABLE ScoreTransaction (
    ScoreTransactionId  INT             NOT NULL    IDENTITY(1, 1),
    UserId              NVARCHAR(25)    NOT NULL,
    TicketId            INT             NOT NULL,
    Points              INT             NOT NULL,
    Reason              TINYINT         NOT NULL,   -- ScoreTransactionReason enum
    CreatedAt           DATETIME2(7)    NOT NULL    DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_ScoreTransaction PRIMARY KEY (ScoreTransactionId),
    CONSTRAINT FK_ScoreTransaction_UserScore_UserId
        FOREIGN KEY (UserId) REFERENCES UserScore (UserId),
    CONSTRAINT FK_ScoreTransaction_Ticket_TicketId
        FOREIGN KEY (TicketId) REFERENCES Ticket (TicketId),
    CONSTRAINT CK_ScoreTransaction_Reason
        CHECK (Reason BETWEEN 0 AND 6)
);
GO

-- ============================================================
-- ÍNDICES
-- ============================================================

-- Department
CREATE INDEX IX_Department_IsEnabled
    ON Department (IsEnabled);

CREATE INDEX IX_Department_CoordinatorUserId
    ON Department (CoordinatorUserId);

-- SlaConfiguration
CREATE INDEX IX_SlaConfiguration_Priority_IsEnabled
    ON SlaConfiguration (Priority, IsEnabled);

-- SupportType
CREATE INDEX IX_SupportType_DepartmentId
    ON SupportType (DepartmentId);

CREATE INDEX IX_SupportType_IsEnabled
    ON SupportType (IsEnabled);

CREATE INDEX IX_SupportType_DepartmentId_IsEnabled
    ON SupportType (DepartmentId, IsEnabled);

-- SupportTypeAgent
CREATE INDEX IX_SupportTypeAgent_SupportTypeId
    ON SupportTypeAgent (SupportTypeId);

CREATE INDEX IX_SupportTypeAgent_UserId
    ON SupportTypeAgent (UserId);

-- Garantiza a nivel de BD que solo un agente puede estar activo por SupportType
CREATE UNIQUE INDEX UX_SupportTypeAgent_SupportTypeId_Active
    ON SupportTypeAgent (SupportTypeId)
    WHERE IsEnabled = 1;

-- Ticket
CREATE INDEX IX_Ticket_DepartmentId
    ON Ticket (DepartmentId);

CREATE INDEX IX_Ticket_SupportTypeId
    ON Ticket (SupportTypeId);

CREATE INDEX IX_Ticket_Status
    ON Ticket (Status);

CREATE INDEX IX_Ticket_Priority
    ON Ticket (Priority);

CREATE INDEX IX_Ticket_AssignedUserId
    ON Ticket (AssignedUserId);

CREATE INDEX IX_Ticket_RelatedTicketId
    ON Ticket (RelatedTicketId)
    WHERE RelatedTicketId IS NOT NULL;

CREATE INDEX IX_Ticket_RequestedAt
    ON Ticket (RequestedAt);

CREATE INDEX IX_Ticket_Deadline
    ON Ticket (Deadline)
    WHERE Deadline IS NOT NULL;

CREATE INDEX IX_Ticket_IsEnabled
    ON Ticket (IsEnabled);

-- Para consultas de tickets activos por estado y prioridad
CREATE INDEX IX_Ticket_Status_IsEnabled_Priority
    ON Ticket (Status, IsEnabled, Priority);

-- Para GetByAgentAsync y GetOverdueSlaAsync
CREATE INDEX IX_Ticket_AssignedUserId_Status_Deadline
    ON Ticket (AssignedUserId, Status, Deadline);

-- TicketComment
CREATE INDEX IX_TicketComment_TicketId
    ON TicketComment (TicketId);

CREATE INDEX IX_TicketComment_AuthorId
    ON TicketComment (AuthorId);

CREATE INDEX IX_TicketComment_TicketId_Visibility
    ON TicketComment (TicketId, Visibility);

-- TicketAttachment
CREATE INDEX IX_TicketAttachment_TicketId
    ON TicketAttachment (TicketId);

CREATE INDEX IX_TicketAttachment_CommentId
    ON TicketAttachment (CommentId)
    WHERE CommentId IS NOT NULL;

-- TicketHistory
CREATE INDEX IX_TicketHistory_TicketId
    ON TicketHistory (TicketId);

CREATE INDEX IX_TicketHistory_TicketId_ExecutedAt
    ON TicketHistory (TicketId, ExecutedAt);

CREATE INDEX IX_TicketHistory_ActionType
    ON TicketHistory (ActionType);

-- UserScore
CREATE INDEX IX_UserScore_Level
    ON UserScore (Level);

-- ScoreTransaction
CREATE INDEX IX_ScoreTransaction_UserId
    ON ScoreTransaction (UserId);

CREATE INDEX IX_ScoreTransaction_TicketId
    ON ScoreTransaction (TicketId);

CREATE INDEX IX_ScoreTransaction_Reason
    ON ScoreTransaction (Reason);

CREATE INDEX IX_ScoreTransaction_UserId_CreatedAt
    ON ScoreTransaction (UserId, CreatedAt);

GO

-- ============================================================
-- VERIFICACIÓN
-- ============================================================
SELECT
    t.name      AS Tabla,
    p.rows      AS Filas
FROM sys.tables t
JOIN sys.partitions p
    ON t.object_id = p.object_id
    AND p.index_id IN (0, 1)
ORDER BY t.name;
GO
