-- ============================================================
-- HelpDesk - Seed Script
-- Datos basados en seed.ts (HelpDeskFront/src/data/seed.ts)
-- Compatible con el esquema de script_inicial_bd.sql
-- Fecha de referencia: 2026-05-06
-- ============================================================

USE HelpDesk;
GO

-- ============================================================
-- LIMPIEZA (orden inverso de FK)
-- ============================================================
DELETE FROM ScoreTransaction;
DELETE FROM TicketHistory;
DELETE FROM TicketAttachment;
DELETE FROM TicketComment;
DELETE FROM Ticket;
DELETE FROM SupportTypeAgent;
DELETE FROM SupportType;
DELETE FROM Department;
DELETE FROM UserScore;
DELETE FROM SlaConfiguration;
GO

-- ============================================================
-- SlaConfiguration
-- Valores tomados de HD_PRIORITIES (seed.ts líneas 51-56)
-- Priority: 0=Critical · 1=High · 2=Medium · 3=Low
-- ============================================================
SET IDENTITY_INSERT SlaConfiguration ON;
INSERT INTO SlaConfiguration (SlaConfigurationId, Priority, HoursLimit, CreatedAt, CreatedBy, IsEnabled)
VALUES
    (1, 0,  2, SYSUTCDATETIME(), 'system', 1),  -- Critical:  2 h
    (2, 1,  8, SYSUTCDATETIME(), 'system', 1),  -- High:      8 h
    (3, 2, 24, SYSUTCDATETIME(), 'system', 1),  -- Medium:   24 h
    (4, 3, 72, SYSUTCDATETIME(), 'system', 1);  -- Low:      72 h
SET IDENTITY_INSERT SlaConfiguration OFF;
GO

-- ============================================================
-- Department
-- Tomados de HD_DEPARTMENTS (seed.ts líneas 7-14)
-- CoordinatorUserId: rocio.zavala coordina Tecnología (D01)
-- ============================================================
SET IDENTITY_INSERT Department ON;
INSERT INTO Department (DepartmentId, Name, Description, CoordinatorUserId, CreatedAt, CreatedBy, IsEnabled)
VALUES
    (1, 'Tecnología',          'Infraestructura, soporte de TI y sistemas',          'rocio.zavala', SYSUTCDATETIME(), 'system', 1),
    (2, 'Recursos Humanos',    'Nómina, vacaciones, constancias y capacitación',       NULL,           SYSUTCDATETIME(), 'system', 1),
    (3, 'Finanzas',            'Reembolsos, pagos a proveedores y viáticos',           NULL,           SYSUTCDATETIME(), 'system', 1),
    (4, 'Operaciones',         'Mantenimiento de planta, líneas y consumibles',        NULL,           SYSUTCDATETIME(), 'system', 1),
    (5, 'Comercial',           'Cotizaciones, postventa y atención a clientes',        NULL,           SYSUTCDATETIME(), 'system', 1),
    (6, 'Servicios Generales', 'Limpieza, mantenimiento de oficinas y transporte',     NULL,           SYSUTCDATETIME(), 'system', 1);
SET IDENTITY_INSERT Department OFF;
GO

-- ============================================================
-- SupportType (20 tipos)
-- Tomados de HD_SUPPORT_TYPES (seed.ts líneas 16-49)
-- DepartmentId → 1=Tecnología 2=RRHH 3=Finanzas 4=Ops 5=Comercial 6=ServGen
-- ============================================================
SET IDENTITY_INSERT SupportType ON;
INSERT INTO SupportType (SupportTypeId, DepartmentId, Name, Description, CreatedAt, CreatedBy, IsEnabled)
VALUES
    -- D01 Tecnología
    ( 1, 1, 'Soporte de hardware',      'Equipos, periféricos y componentes físicos',    SYSUTCDATETIME(), 'system', 1),
    ( 2, 1, 'Soporte de software',      'Aplicaciones, licencias y sistema operativo',   SYSUTCDATETIME(), 'system', 1),
    ( 3, 1, 'Acceso y permisos',        'Cuentas, carpetas compartidas y VPN',            SYSUTCDATETIME(), 'system', 1),
    ( 4, 1, 'Conectividad / red',       'Fallas de red, Wi-Fi y conectividad',            SYSUTCDATETIME(), 'system', 1),
    ( 5, 1, 'Solicitud de equipo',      'Laptop, monitor y accesorios nuevos',            SYSUTCDATETIME(), 'system', 1),
    -- D02 Recursos Humanos
    ( 6, 2, 'Recibo de nómina',         'Consultas y correcciones de nómina',             SYSUTCDATETIME(), 'system', 1),
    ( 7, 2, 'Permisos y vacaciones',    'Solicitud y gestión de permisos',                SYSUTCDATETIME(), 'system', 1),
    ( 8, 2, 'Constancias y certificados','Constancia laboral y carta de recomendación',   SYSUTCDATETIME(), 'system', 1),
    ( 9, 2, 'Capacitación',             'Inscripción y gestión de cursos',                SYSUTCDATETIME(), 'system', 1),
    -- D03 Finanzas
    (10, 3, 'Reembolsos',               'Solicitud de reembolso de gastos',               SYSUTCDATETIME(), 'system', 1),
    (11, 3, 'Pago a proveedores',       'Gestión y seguimiento de pagos',                 SYSUTCDATETIME(), 'system', 1),
    (12, 3, 'Anticipos y viáticos',     'Solicitud de anticipos para viajes',             SYSUTCDATETIME(), 'system', 1),
    -- D04 Operaciones
    (13, 4, 'Mantenimiento de planta',  'Mantenimiento preventivo y correctivo',          SYSUTCDATETIME(), 'system', 1),
    (14, 4, 'Falla de línea',           'Paros de producción y alarmas críticas',         SYSUTCDATETIME(), 'system', 1),
    (15, 4, 'Insumos y consumibles',    'Reposición de materiales e insumos',             SYSUTCDATETIME(), 'system', 1),
    -- D05 Comercial
    (16, 5, 'Cotización a cliente',     'Elaboración de propuestas comerciales',          SYSUTCDATETIME(), 'system', 1),
    (17, 5, 'Postventa',               'Soporte y seguimiento post-venta',                SYSUTCDATETIME(), 'system', 1),
    -- D06 Servicios Generales
    (18, 6, 'Limpieza y aseo',          'Servicios de limpieza y sanitización',           SYSUTCDATETIME(), 'system', 1),
    (19, 6, 'Mantenimiento de oficina', 'Reparaciones y adecuaciones de espacio',         SYSUTCDATETIME(), 'system', 1),
    (20, 6, 'Vehículos y transporte',   'Gestión de flota y solicitudes de viaje',        SYSUTCDATETIME(), 'system', 1);
SET IDENTITY_INSERT SupportType OFF;
GO

-- ============================================================
-- SupportTypeAgent
-- Un agente activo por tipo de soporte.
-- Agentes de HD_PEOPLE (seed.ts líneas 71-78): alvaro, lucia, carola, hector, diego, ana
-- ============================================================
SET IDENTITY_INSERT SupportTypeAgent ON;
INSERT INTO SupportTypeAgent (SupportTypeAgentId, SupportTypeId, UserId, CreatedAt, CreatedBy, IsEnabled)
VALUES
    -- D01 Tecnología
    ( 1,  1, 'alvaro.duarte', SYSUTCDATETIME(), 'system', 1),  -- hardware
    ( 2,  2, 'alvaro.duarte', SYSUTCDATETIME(), 'system', 1),  -- software
    ( 3,  3, 'lucia.morales', SYSUTCDATETIME(), 'system', 1),  -- acceso y permisos
    ( 4,  4, 'lucia.morales', SYSUTCDATETIME(), 'system', 1),  -- conectividad/red
    ( 5,  5, 'alvaro.duarte', SYSUTCDATETIME(), 'system', 1),  -- solicitud equipo
    -- D02 Recursos Humanos
    ( 6,  6, 'carola.reyes',  SYSUTCDATETIME(), 'system', 1),
    ( 7,  7, 'carola.reyes',  SYSUTCDATETIME(), 'system', 1),
    ( 8,  8, 'carola.reyes',  SYSUTCDATETIME(), 'system', 1),
    ( 9,  9, 'carola.reyes',  SYSUTCDATETIME(), 'system', 1),
    -- D03 Finanzas
    (10, 10, 'hector.medina', SYSUTCDATETIME(), 'system', 1),
    (11, 11, 'hector.medina', SYSUTCDATETIME(), 'system', 1),
    (12, 12, 'hector.medina', SYSUTCDATETIME(), 'system', 1),
    -- D04 Operaciones
    (13, 13, 'diego.salinas', SYSUTCDATETIME(), 'system', 1),
    (14, 14, 'diego.salinas', SYSUTCDATETIME(), 'system', 1),
    (15, 15, 'diego.salinas', SYSUTCDATETIME(), 'system', 1),
    -- D05 Comercial
    (16, 16, 'alvaro.duarte', SYSUTCDATETIME(), 'system', 1),
    (17, 17, 'alvaro.duarte', SYSUTCDATETIME(), 'system', 1),
    -- D06 Servicios Generales
    (18, 18, 'ana.solis',     SYSUTCDATETIME(), 'system', 1),
    (19, 19, 'ana.solis',     SYSUTCDATETIME(), 'system', 1),
    (20, 20, 'ana.solis',     SYSUTCDATETIME(), 'system', 1);
SET IDENTITY_INSERT SupportTypeAgent OFF;
GO

-- ============================================================
-- UserScore
-- Level: 0=Bronze · 1=Silver · 2=Gold
-- Solicitantes de HD_PEOPLE + dev-user (DevBypass)
-- Agentes del HD_LEADERBOARD (seed.ts líneas 245-252)
-- ============================================================
SET IDENTITY_INSERT UserScore ON;
INSERT INTO UserScore (UserScoreId, UserId, CurrentPoints, Level)
VALUES
    -- Usuario de prueba del DevBypass
    ( 1, 'dev-user',      0,    0),  -- Bronze
    -- Solicitantes (HD_PEOPLE)
    ( 2, 'marina.galvez', 1480, 1),  -- Silver  (HD_SCORE_MARINA)
    ( 3, 'esteban.rios',  620,  0),  -- Bronze
    ( 4, 'paola.benitez', 890,  0),  -- Bronze
    ( 5, 'fernando.cano', 340,  0),  -- Bronze
    -- Agentes (HD_LEADERBOARD)
    ( 6, 'alvaro.duarte', 4280, 2),  -- Gold
    ( 7, 'lucia.morales', 3940, 2),  -- Gold
    ( 8, 'diego.salinas', 3610, 2),  -- Gold
    ( 9, 'carola.reyes',  3120, 1),  -- Silver
    (10, 'hector.medina', 2880, 1),  -- Silver
    (11, 'ana.solis',     2410, 1);  -- Silver
SET IDENTITY_INSERT UserScore OFF;
GO

-- ============================================================
-- Ticket
-- Priority  : 0=Critical · 1=High · 2=Medium · 3=Low
-- Status    : 0=Open · 1=InProgress · 2=WaitingForInfo · 3=Closed · 4=Reopened
-- ResolutionCategory: obligatorio cuando Status=3 → 0=Resolved
-- Deadline  : RequestedAt + horas SLA de la prioridad
-- ============================================================
SET IDENTITY_INSERT Ticket ON;
INSERT INTO Ticket (
    TicketId, TicketNumber, Subject, Description,
    DepartmentId, SupportTypeId, Priority, Status, ResolutionCategory,
    AssignedUserId, RequestedAt, FirstOpenedAt, WorkStartedAt, ClosedAt,
    Deadline, PausedAt, TotalPausedMinutes, CreatedAt, CreatedBy, IsEnabled
)
VALUES

    -- ================================================================
    -- Tickets del usuario de prueba (dev-user)
    -- Necesarios para que el Dashboard Solicitante muestre datos reales
    -- ================================================================

    -- TK-1001 · Abierto · Hardware · High (SLA 8 h)
    (1, 1001,
     'Mouse inalámbrico no responde',
     'El mouse dejó de responder después de cambiar las baterías. Ya probé con baterías nuevas y en otro equipo. Modelo: Logitech MX Master 3.',
     1, 1, 1, 0, NULL,
     'alvaro.duarte',
     '2026-05-06T08:00:00', NULL, NULL, NULL,
     '2026-05-06T16:00:00', NULL, 0,
     '2026-05-06T08:00:00', 'dev-user', 1),

    -- TK-1002 · En proceso · Acceso · Medium (SLA 24 h)
    (2, 1002,
     'Sin acceso a carpeta \\fileserver\Proyectos',
     'Necesito acceso de lectura a la carpeta de proyectos para colaborar con el equipo de Tecnología.',
     1, 3, 2, 1, NULL,
     'lucia.morales',
     '2026-05-05T14:30:00', '2026-05-05T15:00:00', '2026-05-05T15:00:00', NULL,
     '2026-05-06T14:30:00', NULL, 0,
     '2026-05-05T14:30:00', 'dev-user', 1),

    -- TK-1003 · Esperando info · Reembolsos · Medium (SLA 24 h, pausado)
    (3, 1003,
     'Reembolso de gastos de traslado — Abril',
     'Solicito reembolso por transporte y alimentación del viaje a Monterrey los días 22-23 de abril. Adjunto comprobantes.',
     3, 10, 2, 2, NULL,
     'hector.medina',
     '2026-05-04T10:00:00', '2026-05-04T10:45:00', '2026-05-04T10:45:00', NULL,
     '2026-05-05T10:00:00', '2026-05-05T09:00:00', 0,
     '2026-05-04T10:00:00', 'dev-user', 1),

    -- TK-1004 · Cerrado · Software · Medium (SLA 24 h, cerrado a tiempo)
    (4, 1004,
     'Excel no abre archivos .xlsx del servidor',
     'Al intentar abrir archivos .xlsx desde la ruta de red, Excel muestra error "no se puede abrir el archivo". Archivos locales sí abren.',
     1, 2, 2, 3, 0,
     'alvaro.duarte',
     '2026-05-02T09:00:00', '2026-05-02T09:30:00', '2026-05-02T09:30:00', '2026-05-02T14:00:00',
     '2026-05-03T09:00:00', NULL, 0,
     '2026-05-02T09:00:00', 'dev-user', 1),

    -- ================================================================
    -- Tickets del seed HD_TICKETS (datos de demo completo)
    -- ================================================================

    -- TK-2284 · marina.galvez · Hardware · InProgress · High
    (5, 2284,
     'Pantalla externa parpadea al conectar al dock',
     'Desde ayer, al conectar mi laptop al dock, la pantalla externa parpadea cada 30–40 segundos. Probé con otro cable HDMI sin éxito. Adjunto fotografía del dock y del modelo de pantalla.',
     1, 1, 1, 1, NULL,
     'alvaro.duarte',
     '2026-05-06T09:14:00', '2026-05-06T09:22:00', '2026-05-06T09:22:00', NULL,
     '2026-05-06T17:14:00', NULL, 0,
     '2026-05-06T09:14:00', 'marina.galvez', 1),

    -- TK-2283 · esteban.rios · Red · Open · Critical
    (6, 2283,
     'Red caída en el piso 4 — sala de juntas',
     'Toda la sala de juntas del piso 4 se quedó sin red. Tenemos una demo con cliente en 1 hora.',
     1, 4, 0, 0, NULL,
     'lucia.morales',
     '2026-05-06T10:03:00', NULL, NULL, NULL,
     '2026-05-06T12:03:00', NULL, 0,
     '2026-05-06T10:03:00', 'esteban.rios', 1),

    -- TK-2282 · fernando.cano · Constancias · InProgress · Low
    (7, 2282,
     'Solicitud de constancia laboral con sello',
     'Necesito constancia laboral con sello para trámite bancario.',
     2, 8, 3, 1, NULL,
     'carola.reyes',
     '2026-05-05T14:20:00', '2026-05-06T09:05:00', '2026-05-06T09:05:00', NULL,
     '2026-05-08T14:20:00', NULL, 0,
     '2026-05-05T14:20:00', 'fernando.cano', 1),

    -- TK-2281 · paola.benitez · Reembolsos · WaitingForInfo · Medium
    (8, 2281,
     'Reembolso de viaje a Querétaro — facturas Sept',
     'Adjunto comprobantes del viaje del 12–14 de septiembre.',
     3, 10, 2, 2, NULL,
     'hector.medina',
     '2026-05-05T11:00:00', '2026-05-05T11:30:00', '2026-05-05T11:30:00', NULL,
     '2026-05-06T11:00:00', '2026-05-06T08:30:00', 0,
     '2026-05-05T11:00:00', 'paola.benitez', 1),

    -- TK-2280 · marina.galvez · Acceso · Closed · Medium
    (9, 2280,
     'Acceso a carpeta compartida \\fileserver\Comercial',
     'Necesito permiso de lectura/escritura para preparar la propuesta del cliente Andesa.',
     1, 3, 2, 3, 0,
     'lucia.morales',
     '2026-05-03T08:00:00', '2026-05-03T08:30:00', '2026-05-03T08:30:00', '2026-05-04T10:00:00',
     '2026-05-04T08:00:00', NULL, 0,
     '2026-05-03T08:00:00', 'marina.galvez', 1),

    -- TK-2279 · esteban.rios · Falla de línea · Closed · Critical
    (10, 2279,
     'Línea 3 detenida — alarma sensor de temperatura',
     'Sensor de temperatura del cabezal de empacado disparó alarma. Línea detenida.',
     4, 14, 0, 3, 0,
     'diego.salinas',
     '2026-05-02T07:00:00', '2026-05-02T07:15:00', '2026-05-02T07:15:00', '2026-05-02T09:30:00',
     '2026-05-02T09:00:00', NULL, 0,
     '2026-05-02T07:00:00', 'esteban.rios', 1),

    -- TK-2278 · marina.galvez · Cotización · InProgress · Medium
    (11, 2278,
     'Cotización para cliente Andesa — 200 unidades',
     'El cliente solicita cotización por 200 unidades del modelo A-12 con entrega a 30 días.',
     5, 16, 2, 1, NULL,
     'alvaro.duarte',
     '2026-05-06T08:00:00', '2026-05-06T08:30:00', '2026-05-06T08:30:00', NULL,
     '2026-05-07T08:00:00', NULL, 0,
     '2026-05-06T08:00:00', 'marina.galvez', 1),

    -- TK-2277 · paola.benitez · Software · Reopened · Medium
    (12, 2277,
     'Outlook no sincroniza calendario compartido',
     'El calendario compartido del equipo dejó de sincronizar desde el lunes.',
     1, 2, 2, 4, NULL,
     'alvaro.duarte',
     '2026-04-30T10:00:00', '2026-04-30T10:30:00', '2026-04-30T10:30:00', NULL,
     '2026-05-07T10:00:00', NULL, 0,
     '2026-04-30T10:00:00', 'paola.benitez', 1),

    -- TK-2276 · fernando.cano · Mantenimiento de oficina · Closed · Low
    (13, 2276,
     'Reservar sala Atlas para junta directiva',
     'Reservar la sala Atlas el viernes 28 de 10:00 a 13:00.',
     6, 19, 3, 3, 0,
     'ana.solis',
     '2026-05-01T09:00:00', '2026-05-01T09:30:00', '2026-05-01T09:30:00', '2026-05-02T14:00:00',
     '2026-05-04T09:00:00', NULL, 0,
     '2026-05-01T09:00:00', 'fernando.cano', 1),

    -- TK-2275 · fernando.cano · Solicitud de equipo · Open · Low (sin asignar)
    (14, 2275,
     'Solicitud de laptop para nuevo desarrollador',
     'El nuevo desarrollador del equipo de Plataforma ingresa el lunes 24, requiere laptop con 32 GB de RAM.',
     1, 5, 3, 0, NULL,
     NULL,
     '2026-05-05T16:40:00', NULL, NULL, NULL,
     '2026-05-08T16:40:00', NULL, 0,
     '2026-05-05T16:40:00', 'fernando.cano', 1);

SET IDENTITY_INSERT Ticket OFF;
GO

-- ============================================================
-- TicketComment
-- Hilo de HD_THREADS['TK-2284'] + comentarios de tickets dev-user
-- Visibility: 0=Public · 1=Internal
-- ============================================================
SET IDENTITY_INSERT TicketComment ON;
INSERT INTO TicketComment (TicketCommentId, TicketId, Content, AuthorId, Visibility, CreatedAt)
VALUES
    -- TK-2284 (TicketId=5) — hilo completo de HD_THREADS
    ( 1, 5, 'Desde ayer, al conectar mi laptop al dock, la pantalla externa parpadea cada 30–40 segundos. Probé con otro cable HDMI sin éxito. Adjunto fotografía del dock y del modelo de pantalla.',
      'marina.galvez', 0, '2026-05-06T09:14:00'),
    ( 2, 5, 'Buenos días Marina, ¿podrías indicarme la marca y modelo de la pantalla externa, y si el parpadeo ocurre también con la pantalla integrada de la laptop? Te agradezco.',
      'alvaro.duarte', 0, '2026-05-06T09:31:00'),
    ( 3, 5, 'Es una Dell U2723QE, sólo parpadea la externa, la integrada está bien. Si desconecto el dock y conecto directo, no parpadea.',
      'marina.galvez', 0, '2026-05-06T09:48:00'),
    ( 4, 5, 'Nota interna: el dock parece ser el WD19TBS, ya hubo dos casos similares (TK-2218, TK-2231). Probable firmware desactualizado.',
      'alvaro.duarte', 1, '2026-05-06T10:05:00'),
    ( 5, 5, 'Marina, encontré el patrón: el firmware del dock está desactualizado. Voy a pasar a tu lugar a las 14:00 para actualizarlo, toma unos 15 minutos. ¿Te queda bien?',
      'alvaro.duarte', 0, '2026-05-06T11:42:00'),

    -- TK-1001 (TicketId=1) — dev-user, hardware
    ( 6, 1, 'El mouse dejó de responder después de cambiar las baterías. Ya probé con baterías nuevas y en otro equipo. Modelo: Logitech MX Master 3.',
      'dev-user', 0, '2026-05-06T08:00:00'),
    ( 7, 1, 'Hola, ¿podrías indicarme en qué puerto USB lo conectas el receptor (USB-A o USB-C) y si el mouse responde al conectarlo directamente por cable?',
      'alvaro.duarte', 0, '2026-05-06T08:35:00'),

    -- TK-1002 (TicketId=2) — dev-user, acceso
    ( 8, 2, 'Necesito acceso de lectura a la carpeta de proyectos para colaborar con el equipo de Tecnología.',
      'dev-user', 0, '2026-05-05T14:30:00'),
    ( 9, 2, 'Recibido. Voy a gestionar los permisos necesarios con el administrador de directorio activo. Te aviso en cuanto esté listo.',
      'lucia.morales', 0, '2026-05-05T15:05:00'),

    -- TK-1003 (TicketId=3) — dev-user, reembolso, esperando info
    (10, 3, 'Solicito reembolso por transporte y alimentación del viaje a Monterrey los días 22-23 de abril. Adjunto comprobantes.',
      'dev-user', 0, '2026-05-04T10:00:00'),
    (11, 3, 'Recibimos tu solicitud. ¿Podrías confirmar si las facturas ya están timbradas en el portal del SAT? Las necesitamos para tramitar el reembolso.',
      'hector.medina', 0, '2026-05-04T10:50:00'),

    -- TK-1004 (TicketId=4) — dev-user, software, cerrado
    (12, 4, 'Al intentar abrir archivos .xlsx desde la ruta de red, Excel muestra error "no se puede abrir el archivo". Archivos locales sí abren sin problemas.',
      'dev-user', 0, '2026-05-02T09:00:00'),
    (13, 4, 'El problema era un bloqueo de archivos por la política de seguridad de zona de red no confiable. Apliqué el ajuste de confianza para esa ruta en las opciones de Centro de confianza de Excel. Prueba abriendo de nuevo.',
      'alvaro.duarte', 0, '2026-05-02T13:45:00'),
    (14, 4, 'Perfecto, ya abre sin problemas. Muchas gracias por la solución tan rápida.',
      'dev-user', 0, '2026-05-02T14:00:00');

SET IDENTITY_INSERT TicketComment OFF;
GO

-- ============================================================
-- TicketHistory
-- Auditoría de ciclo de vida para tickets clave
-- HD_HISTORY['TK-2284'] + tickets dev-user
-- ============================================================
SET IDENTITY_INSERT TicketHistory ON;
INSERT INTO TicketHistory (TicketHistoryId, TicketId, ActionType, PreviousValue, NewValue, ExecutedBy, ExecutedAt)
VALUES
    -- TK-2284 (TicketId=5) — hilo de HD_HISTORY
    ( 1,  5, 'Created',      NULL,          'Open',          'marina.galvez', '2026-05-06T09:14:00'),
    ( 2,  5, 'Assigned',     NULL,          'alvaro.duarte', 'marina.galvez', '2026-05-06T09:22:00'),
    ( 3,  5, 'StatusChange', 'Open',        'InProgress',    'alvaro.duarte', '2026-05-06T09:23:00'),

    -- TK-1001 dev-user (TicketId=1)
    ( 4,  1, 'Created',      NULL,          'Open',          'dev-user',      '2026-05-06T08:00:00'),

    -- TK-1002 dev-user (TicketId=2)
    ( 5,  2, 'Created',      NULL,          'Open',          'dev-user',      '2026-05-05T14:30:00'),
    ( 6,  2, 'StatusChange', 'Open',        'InProgress',    'lucia.morales', '2026-05-05T15:00:00'),

    -- TK-1003 dev-user (TicketId=3)
    ( 7,  3, 'Created',      NULL,          'Open',          'dev-user',      '2026-05-04T10:00:00'),
    ( 8,  3, 'StatusChange', 'Open',        'InProgress',    'hector.medina', '2026-05-04T10:45:00'),
    ( 9,  3, 'StatusChange', 'InProgress',  'WaitingForInfo','hector.medina', '2026-05-05T09:00:00'),

    -- TK-1004 dev-user (TicketId=4)
    (10,  4, 'Created',      NULL,          'Open',          'dev-user',      '2026-05-02T09:00:00'),
    (11,  4, 'StatusChange', 'Open',        'InProgress',    'alvaro.duarte', '2026-05-02T09:30:00'),
    (12,  4, 'Closed',       'InProgress',  'Closed',        'alvaro.duarte', '2026-05-02T14:00:00'),

    -- TK-2277 Reopened (TicketId=12)
    (13, 12, 'Created',      NULL,          'Open',          'paola.benitez', '2026-04-30T10:00:00'),
    (14, 12, 'StatusChange', 'Open',        'InProgress',    'alvaro.duarte', '2026-04-30T10:30:00'),
    (15, 12, 'Closed',       'InProgress',  'Closed',        'alvaro.duarte', '2026-05-02T16:00:00'),
    (16, 12, 'Reopened',     'Closed',      'Reopened',      'paola.benitez', '2026-05-06T07:50:00'),

    -- TK-2280 Closed (TicketId=9)
    (17,  9, 'Created',      NULL,          'Open',          'marina.galvez', '2026-05-03T08:00:00'),
    (18,  9, 'StatusChange', 'Open',        'InProgress',    'lucia.morales', '2026-05-03T08:30:00'),
    (19,  9, 'Closed',       'InProgress',  'Closed',        'lucia.morales', '2026-05-04T10:00:00'),

    -- TK-2279 Closed (TicketId=10)
    (20, 10, 'Created',      NULL,          'Open',          'esteban.rios',  '2026-05-02T07:00:00'),
    (21, 10, 'StatusChange', 'Open',        'InProgress',    'diego.salinas', '2026-05-02T07:15:00'),
    (22, 10, 'Closed',       'InProgress',  'Closed',        'diego.salinas', '2026-05-02T09:30:00');

SET IDENTITY_INSERT TicketHistory OFF;
GO

-- ============================================================
-- VERIFICACIÓN FINAL
-- ============================================================
SELECT 'SlaConfiguration'  AS Tabla, COUNT(*) AS Filas FROM SlaConfiguration
UNION ALL SELECT 'Department',       COUNT(*) FROM Department
UNION ALL SELECT 'SupportType',      COUNT(*) FROM SupportType
UNION ALL SELECT 'SupportTypeAgent', COUNT(*) FROM SupportTypeAgent
UNION ALL SELECT 'UserScore',        COUNT(*) FROM UserScore
UNION ALL SELECT 'Ticket',           COUNT(*) FROM Ticket
UNION ALL SELECT 'TicketComment',    COUNT(*) FROM TicketComment
UNION ALL SELECT 'TicketHistory',    COUNT(*) FROM TicketHistory;
GO
