# 📖 Product Backlog — Sistema de Gestión de Tickets de Soporte
### Versión Refinada para Desarrollo

> **Estado:** Listo para Sprint Planning  
> **Versión:** 3.0 — Épica 8 Dashboard agregada  
> **Fecha:** Abril 2026

---

## 🗂️ Índice

- [Épica 1: Creación y Enrutamiento Inicial](#épica-1)
- [Épica 2: Gestión y Atención por el Soporte](#épica-2)
- [Épica 3: Comunicación e Iteración](#épica-3)
- [Épica 4: Cierre, Calificación y Trazabilidad](#épica-4)
- [Épica 5: Administración y Catálogos](#épica-5)
- [Épica 6: Gamificación y Reputación](#épica-6)
- [Épica 7: Automatización y Mantenimiento](#épica-7)
- [Épica 8: Dashboard Operativo e Inteligencia del Sistema](#épica-8)

---

<a name="épica-1"></a>
# 🗂️ ÉPICA 1: Creación y Enrutamiento Inicial

**Objetivo:** Permitir a los usuarios registrar sus solicitudes de soporte de forma estructurada, asegurando que cada ticket llegue al especialista correcto con un tiempo límite de atención claramente calculado.

---

### 🎫 US-01 — Creación de Ticket con Asignación y Cálculo de SLA

**Como** usuario del sistema,  
**Quiero** registrar un ticket completando un formulario con el departamento, tipo de soporte, prioridad y descripción de mi problema,  
**Para que** el equipo correcto reciba mi solicitud y yo sepa en qué tiempo será atendida.

#### 📌 Criterios de Aceptación

**Escenario 1 — Carga dinámica del formulario**
- **Dado** que el usuario está en el formulario de nuevo ticket y selecciona un Departamento
- **Cuando** el Departamento es seleccionado
- **Entonces** el campo "Tipo de Soporte" se actualiza automáticamente mostrando solo las opciones vinculadas a ese departamento, y las opciones de otros departamentos desaparecen de la lista

**Escenario 2 — Creación exitosa y estado inicial**
- **Dado** que el usuario completa todos los campos obligatorios (Departamento, Tipo de Soporte, Prioridad, Asunto, Descripción)
- **Cuando** guarda el ticket
- **Entonces** el ticket se crea con estado "Abierto" y el sistema registra la fecha y hora exacta de creación

**Escenario 3 — Cálculo automático de fecha límite**
- **Dado** que el usuario selecciona Prioridad "Alta" (configurada en 4 horas) y guarda a las 09:00:00
- **Cuando** el sistema procesa la solicitud
- **Entonces** calcula la fecha límite como 13:00:00 del mismo día

**Escenario 4 — Asignación automática al agente por defecto**
- **Dado** que el Tipo de Soporte seleccionado tiene un agente por defecto configurado
- **Cuando** el ticket se crea exitosamente
- **Entonces** el ticket queda asignado a ese agente y él recibe una notificación

**Escenario 5 — Sin agente por defecto configurado**
- **Dado** que el Tipo de Soporte no tiene agente por defecto
- **Cuando** el ticket se crea
- **Entonces** queda en bandeja general y el coordinador recibe alerta para asignación manual

#### 📋 Notas adicionales
- Campos obligatorios: Departamento, Tipo de Soporte, Prioridad, Asunto, Descripción
- La fecha de creación es inmutable una vez guardada
- El cálculo del tiempo usa horas calendario

---

### 📎 US-02 — Adjuntar Evidencia en la Creación del Ticket

**Como** usuario del sistema,  
**Quiero** adjuntar uno o varios archivos a mi ticket al momento de crearlo,  
**Para** proporcionar contexto visual o técnico (logs, capturas de pantalla, documentos) que agilice la resolución.

#### 📌 Criterios de Aceptación

**Escenario 1 — Adjunto válido**
- **Dado** que el usuario selecciona un archivo de 2 MB en formato .pdf
- **Cuando** lo adjunta al formulario
- **Entonces** el archivo se agrega a la lista de adjuntos y queda vinculado al ticket al guardar

**Escenario 2 — Rechazo por tamaño**
- **Dado** que el usuario selecciona un archivo de 15 MB
- **Cuando** intenta adjuntarlo
- **Entonces** el sistema rechaza el archivo con el mensaje: *"El archivo supera el límite permitido de 10 MB"*

**Escenario 3 — Rechazo por tipo de archivo**
- **Dado** que el usuario selecciona un archivo con extensión bloqueada (ej. .exe)
- **Cuando** intenta adjuntarlo
- **Entonces** el sistema rechaza el archivo con el mensaje: *"Tipo de archivo no permitido"*

**Escenario 4 — Visualización en detalle**
- **Dado** que un ticket tiene archivos adjuntos
- **Cuando** cualquier usuario autorizado abre el detalle del ticket
- **Entonces** los archivos se muestran como enlaces de descarga con nombre y tamaño visible

#### 📋 Notas adicionales
- Los adjuntos son opcionales
- Se permiten múltiples archivos por ticket
- Las extensiones bloqueadas son configurables por el administrador

---

<a name="épica-2"></a>
# 🛠️ ÉPICA 2: Gestión y Atención por el Soporte

**Objetivo:** Dotar a los agentes de soporte de las herramientas necesarias para tomar, trabajar, escalar y redirigir tickets eficientemente, con visibilidad de tiempos SLA en todo momento.

---

### 👁️ US-03 — Registro Automático de Primera Apertura del Ticket

**Como** agente de soporte,  
**Quiero** que el sistema registre automáticamente la primera vez que abro un ticket asignado a mí,  
**Para** que quede trazabilidad de cuándo tomé conocimiento oficial del problema.

#### 📌 Criterios de Aceptación

**Escenario 1 — Primera apertura registrada**
- **Dado** que el ticket tiene el campo de primera apertura vacío
- **Cuando** el agente asignado abre el detalle por primera vez
- **Entonces** el sistema registra la fecha y hora exacta automáticamente, sin acción del agente

**Escenario 2 — Segunda apertura no sobreescribe**
- **Dado** que el ticket ya tiene registrada la primera apertura
- **Cuando** el mismo agente vuelve a abrir el ticket
- **Entonces** el campo permanece sin cambios

**Escenario 3 — Apertura de usuario no asignado**
- **Dado** que un supervisor o admin abre el ticket
- **Cuando** se carga la vista de detalle
- **Entonces** el sistema NO registra ni modifica la fecha de primera apertura

---

### ▶️ US-04 — Inicio Formal de Trabajo en el Ticket

**Como** agente de soporte,  
**Quiero** cambiar el estado del ticket a "En Proceso",  
**Para** indicar al usuario que su solicitud está siendo atendida activamente.

#### 📌 Criterios de Aceptación

**Escenario 1 — Primer paso a "En Proceso"**
- **Dado** que el ticket está en estado "Abierto"
- **Cuando** el agente lo cambia a "En Proceso"
- **Entonces** el sistema registra la fecha y hora exacta de inicio de trabajo y genera un registro de auditoría con el detalle del cambio

**Escenario 2 — Regreso a "En Proceso" desde "Esperando Información"**
- **Dado** que el ticket ya tuvo una fecha de inicio de trabajo registrada y regresa a "En Proceso"
- **Cuando** se procesa el cambio de estado
- **Entonces** la fecha original de inicio de trabajo NO se modifica, y se genera un nuevo registro de auditoría

---

### 🔀 US-05 — Redirección de Ticket al Especialista Correcto

**Como** agente de soporte,  
**Quiero** reasignar un ticket a otro tipo de soporte dentro del mismo departamento,  
**Para** enviarlo al especialista correcto cuando el usuario lo categorizó incorrectamente.

#### 📌 Criterios de Aceptación

**Escenario 1 — Catálogo filtrado por departamento**
- **Dado** que el agente accede a la opción de redirección
- **Cuando** se despliegan las opciones disponibles
- **Entonces** solo se muestran los Tipos de Soporte del mismo departamento del ticket

**Escenario 2 — Redirección exitosa**
- **Dado** que el agente selecciona un Tipo de Soporte válido
- **Cuando** confirma la redirección
- **Entonces** el ticket se asigna al agente por defecto del nuevo tipo de soporte, el nuevo agente recibe notificación, y se genera registro de auditoría

**Escenario 3 — Penalización aplicada (primera redirección)**
- **Dado** que es la primera redirección del ticket
- **Cuando** la redirección se confirma
- **Entonces** el sistema aplica la penalización de reputación menor al usuario creador

**Escenario 4 — Sin doble penalización**
- **Dado** que el ticket ya fue redirigido al menos una vez antes
- **Cuando** se redirige nuevamente
- **Entonces** el usuario NO recibe penalización adicional por esta redirección

---

### 🚦 US-12 — Semáforo de SLA en la Bandeja de Entrada

**Como** agente de soporte,  
**Quiero** ver un indicador visual con código de colores y el tiempo restante para cada ticket en mi bandeja,  
**Para** priorizar mi trabajo y atender primero los tickets más urgentes.

#### 📌 Criterios de Aceptación

**Escenario 1 — Indicador Verde (A tiempo)**
- **Dado** que un ticket tiene más del 25% de su tiempo SLA restante
- **Cuando** el agente visualiza su bandeja
- **Entonces** el ticket muestra un indicador VERDE con el tiempo restante en formato legible (ej. "3h 20min")

**Escenario 2 — Indicador Amarillo (En riesgo)**
- **Dado** que un ticket tiene entre 0% y 25% de su tiempo SLA restante y no ha vencido
- **Cuando** el agente visualiza su bandeja
- **Entonces** el ticket muestra indicador AMARILLO

**Escenario 3 — Indicador Rojo (Vencido)**
- **Dado** que la fecha actual supera la fecha límite y el ticket no está cerrado
- **Cuando** el agente visualiza su bandeja
- **Entonces** el ticket muestra indicador ROJO con tiempo en negativo (ej. "-2h 15min")

**Escenario 4 — Ticket pausado**
- **Dado** que un ticket está en estado "Esperando Información"
- **Cuando** el agente visualiza su bandeja
- **Entonces** el indicador se muestra en estado neutro/congelado reflejando que el SLA está pausado

---

### 🚨 US-13 — Escalamiento Automático por SLA Vencido

**Como** coordinador de soporte,  
**Quiero** que el sistema detecte tickets que superaron su fecha límite sin resolverse y notifique al supervisor del área,  
**Para** asegurar una respuesta de escalamiento ante incumplimientos.

#### 📌 Criterios de Aceptación

**Escenario 1 — Detección y marcado de SLA incumplido**
- **Dado** que la fecha actual supera la fecha límite de un ticket activo
- **Cuando** el proceso de verificación se ejecuta
- **Entonces** el ticket se marca internamente como "SLA Incumplido"

**Escenario 2 — Notificación al supervisor**
- **Dado** que un ticket fue marcado como "SLA Incumplido"
- **Cuando** se procesa la notificación
- **Entonces** el supervisor del departamento recibe una alerta con: número de ticket, solicitante, fecha límite, tiempo de atraso

**Escenario 3 — No escalamiento en tickets pausados**
- **Dado** que un ticket está en "Esperando Información" (SLA pausado)
- **Cuando** se ejecuta el proceso de verificación
- **Entonces** el ticket NO es marcado como incumplido, el tiempo de pausa es descontado

---

<a name="épica-3"></a>
# 💬 ÉPICA 3: Comunicación e Iteración

**Objetivo:** Facilitar la comunicación bidireccional entre agentes y usuarios, con gestión inteligente del impacto de cada pausa en el tiempo de respuesta comprometido.

---

### ⏸️ US-06 — Solicitud de Información Adicional y Pausa del SLA

**Como** agente de soporte,  
**Quiero** cambiar el estado del ticket a "Esperando Información" e incluir un comentario explicativo,  
**Para** pausar el conteo del tiempo de respuesta mientras espero que el usuario aclare su solicitud.

#### 📌 Criterios de Aceptación

**Escenario 1 — Pausa exitosa con comentario**
- **Dado** que el agente escribe un comentario y cambia el estado a "Esperando Información"
- **Cuando** el cambio se confirma
- **Entonces** el sistema registra la fecha y hora exacta de inicio de pausa, el SLA queda suspendido y el usuario recibe notificación

**Escenario 2 — Bloqueo sin comentario**
- **Dado** que el agente intenta cambiar a "Esperando Información" sin comentario
- **Cuando** intenta confirmar
- **Entonces** el sistema bloquea la acción con el mensaje: *"Debe agregar un comentario explicando qué información requiere"*

---

### ▶️ US-07 — Respuesta del Usuario y Reanudación del SLA

**Como** usuario del sistema,  
**Quiero** responder al agente añadiendo comentarios o archivos en el hilo del ticket,  
**Para** proporcionar la información solicitada y retomar el proceso de resolución.

#### 📌 Criterios de Aceptación

**Escenario 1 — Reactivación automática del ticket**
- **Dado** que el ticket está en "Esperando Información"
- **Cuando** el usuario agrega un comentario de respuesta
- **Entonces** el estado cambia automáticamente a "En Proceso" y se registra la fecha de reanudación

**Escenario 2 — Recálculo de fecha límite**
- **Dado** que el ticket estuvo pausado 3 horas 30 minutos
- **Cuando** el usuario responde y el estado vuelve a "En Proceso"
- **Entonces** la fecha límite se extiende en exactamente 3h 30min respecto a la fecha límite anterior

**Escenario 3 — Visualización cronológica del hilo**
- **Dado** que un ticket tiene múltiples comentarios de distintas fechas
- **Cuando** el usuario o agente visualiza el hilo
- **Entonces** los comentarios se muestran en orden cronológico ascendente (del más antiguo al más reciente)

---

### 🔒 US-08 — Notas Internas para el Equipo de Soporte

**Como** agente de soporte,  
**Quiero** agregar comentarios marcados como "Nota Interna",  
**Para** documentar hallazgos técnicos, decisiones o contexto que solo el equipo de soporte debe ver.

#### 📌 Criterios de Aceptación

**Escenario 1 — Creación de nota interna**
- **Dado** que el agente activa la opción "Nota Interna" y escribe su comentario
- **Cuando** lo guarda
- **Entonces** se almacena con visibilidad restringida y se muestra con un estilo visual diferenciado en el hilo

**Escenario 2 — Ocultamiento al solicitante**
- **Dado** que un ticket tiene notas internas
- **Cuando** el usuario solicitante visualiza el ticket
- **Entonces** las notas internas NO son visibles ni detectables en su interfaz

**Escenario 3 — Visibilidad para equipo de soporte**
- **Dado** que un agente o administrador visualiza el ticket
- **Cuando** se carga el hilo de comunicación
- **Entonces** las notas internas son visibles con estilo diferenciado (ej. fondo de color distinto)

---

<a name="épica-4"></a>
# ✅ ÉPICA 4: Cierre, Calificación y Trazabilidad

**Objetivo:** Formalizar el cierre de tickets con categorización obligatoria, permitir que los usuarios califiquen el servicio recibido y garantizar trazabilidad completa de todo el ciclo de vida.

---

### 🔐 US-09 — Cierre Formal del Ticket con Categoría de Resolución

**Como** agente de soporte,  
**Quiero** cerrar un ticket seleccionando una categoría de resolución del catálogo,  
**Para** documentar el resultado del ticket y notificar formalmente al usuario.

#### 📌 Criterios de Aceptación

**Escenario 1 — Cierre exitoso**
- **Dado** que el agente selecciona una categoría de resolución válida
- **Cuando** confirma el cierre
- **Entonces** el estado cambia a "Cerrado", se registra la fecha de cierre exacta y el usuario recibe notificación con la resolución aplicada

**Escenario 2 — Bloqueo sin categoría**
- **Dado** que el agente intenta cerrar sin seleccionar categoría
- **Cuando** intenta confirmar
- **Entonces** el sistema bloquea la acción: *"Debe seleccionar una categoría de resolución"*

**Escenario 3 — Categorías disponibles del catálogo**
El catálogo debe incluir al menos:
- ✅ **Resuelto** — Problema solucionado satisfactoriamente
- ❌ **Rechazado** — Solicitud no procede (dispara penalización al usuario)
- 🔁 **Duplicado** — Ticket ya registrado anteriormente (dispara penalización al usuario)
- 🚫 **Cerrado - Sin respuesta** — Cierre automático por inactividad (solo para el sistema)

---

### ⭐ US-10 — Calificación del Servicio por el Usuario

**Como** usuario del sistema,  
**Quiero** calificar la atención recibida y dejar un comentario final en tickets cerrados,  
**Para** dar retroalimentación sobre la calidad del soporte.

#### 📌 Criterios de Aceptación

**Escenario 1 — Calificación disponible para tickets cerrados**
- **Dado** que el ticket está en estado "Cerrado"
- **Cuando** el usuario visualiza el detalle
- **Entonces** la sección de calificación está habilitada y visible

**Escenario 2 — Opción no disponible en tickets activos**
- **Dado** que el ticket está en cualquier estado diferente a "Cerrado"
- **Cuando** el usuario visualiza el ticket
- **Entonces** la sección de calificación NO aparece

**Escenario 3 — Una sola calificación por ticket**
- **Dado** que el usuario ya calificó el ticket
- **Cuando** vuelve a visualizarlo
- **Entonces** su calificación se muestra en modo solo lectura, sin opción de modificar

---

### 🔍 US-11 — Historial de Auditoría Completo del Ticket

**Como** administrador o auditor,  
**Quiero** acceder a un historial detallado de todas las acciones realizadas sobre un ticket,  
**Para** poder auditar el ciclo de vida completo del ticket y verificar quién hizo qué y cuándo.

#### 📌 Criterios de Aceptación

**Escenario 1 — Visualización del historial**
- **Dado** que el usuario con rol de auditor accede a la pestaña "Historial" de un ticket
- **Cuando** se carga la vista
- **Entonces** se muestra una tabla con: Fecha/Hora, Usuario (o "Sistema"), Tipo de Acción, Valor Anterior, Valor Nuevo

**Escenario 2 — Acciones del sistema registradas**
- **Dado** que el sistema realizó una acción automática (ej. auto-cierre, escalamiento)
- **Cuando** se registra en el historial
- **Entonces** el campo "Usuario" muestra "Sistema" en lugar de un nombre de persona

**Escenario 3 — Historial es inmutable**
- **Dado** que el historial contiene registros de auditoría
- **Cuando** cualquier usuario intenta modificar o eliminar una entrada
- **Entonces** el sistema no permite ninguna modificación (vista de solo lectura)

---

<a name="épica-5"></a>
# ⚙️ ÉPICA 5: Administración y Catálogos

**Objetivo:** Permitir la configuración de parámetros del sistema sin necesidad de cambios en el código, dando flexibilidad a los administradores para adaptar el sistema a las políticas de la empresa.

---

### ⏱️ US-14 — Configuración Administrativa de Tiempos SLA por Prioridad

**Como** administrador del sistema,  
**Quiero** configurar el tiempo máximo de resolución (en horas) para cada nivel de prioridad desde una interfaz administrativa,  
**Para** adaptar los acuerdos de nivel de servicio a las políticas de la empresa sin modificar código.

#### 📌 Criterios de Aceptación

**Escenario 1 — Modificación de tiempo SLA**
- **Dado** que el administrador accede al panel de gestión de prioridades
- **Cuando** cambia el valor de "Prioridad Media" de 24 horas a 16 horas y guarda
- **Entonces** el nuevo valor queda activo para todos los tickets creados a partir de ese momento

**Escenario 2 — No retroactividad**
- **Dado** que existe un ticket creado con el valor anterior de 24 horas
- **Cuando** el administrador cambia la configuración a 16 horas
- **Entonces** la fecha límite del ticket existente NO se modifica

**Escenario 3 — Validación de valores**
- **Dado** que el administrador ingresa 0, un número negativo o texto no numérico
- **Cuando** intenta guardar
- **Entonces** el sistema rechaza el cambio con mensaje de validación

---

<a name="épica-6"></a>
# 🏆 ÉPICA 6: Gamificación y Reputación del Solicitante

**Objetivo:** Incentivar el buen uso del sistema y la participación activa de los usuarios mediante un sistema de puntos, niveles y reconocimiento, fomentando la calidad en la categorización de tickets y la retroalimentación continua.

---

### 🎖️ US-15 — Panel de Reputación del Usuario

**Como** usuario del sistema,  
**Quiero** ver mi nivel de reputación actual, mis puntos acumulados y mi progreso hacia el siguiente nivel en mi panel principal,  
**Para** conocer mi posición en la plataforma y estar motivado a mantener buenas prácticas.

#### 📌 Criterios de Aceptación

**Escenario 1 — Visualización del panel de reputación**
- **Dado** que el usuario accede a su panel principal
- **Cuando** el sistema carga el componente de reputación
- **Entonces** se muestra: nivel actual con ícono/medalla, puntos acumulados, barra de progreso con puntos necesarios para el siguiente nivel

**Escenario 2 — Detalle de historial de puntos**
- **Dado** que el usuario hace clic en su indicador de reputación
- **Cuando** se abre el detalle
- **Entonces** se muestra una tabla con: fecha, motivo, puntos ganados/perdidos, saldo acumulado

---

### 🎁 US-16 — Otorgamiento de Puntos por Calificar un Ticket

**Como** sistema automatizado,  
**Quiero** otorgar puntos de reputación al usuario cuando califica un ticket cerrado,  
**Para** incentivar la retroalimentación sobre el servicio recibido.

#### 📌 Criterios de Aceptación

**Escenario 1 — Puntos base por calificar**
- **Dado** que el usuario envía una calificación para un ticket cerrado
- **Cuando** el sistema procesa la acción
- **Entonces** se suman X puntos a su saldo y se registra la transacción con motivo "Calificación enviada"

**Escenario 2 — Bono por comentario adicional**
- **Dado** que el usuario califica el ticket E incluye texto en el campo de comentarios
- **Cuando** el sistema procesa la calificación
- **Entonces** se suman X puntos base más Y puntos de bono, ambos registrados como transacciones separadas

**Escenario 3 — Sin puntos por calificación duplicada**
- **Dado** que el usuario ya calificó el ticket (y el sistema bloqueó la segunda calificación)
- **Cuando** se intenta procesar
- **Entonces** NO se generan puntos adicionales

---

### ⚠️ US-17 — Penalización por Tickets Inválidos o Mal Categorizados

**Como** sistema automatizado,  
**Quiero** restar puntos de reputación al usuario cuando su ticket es cerrado con resolución negativa o es redirigido por mala categorización,  
**Para** desincentivar el uso irresponsable del sistema de soporte.

#### 📌 Criterios de Aceptación

**Escenario 1 — Penalización mayor: cierre con resolución negativa**
- **Dado** que un agente cierra el ticket con categoría "Rechazado" o "Duplicado"
- **Cuando** se procesa el cierre
- **Entonces** el sistema resta X puntos (Penalización Mayor) al usuario y registra la transacción con motivo específico

**Escenario 2 — Penalización menor: primera redirección**
- **Dado** que el ticket es redirigido por el agente y es la primera redirección del ticket
- **Cuando** se confirma la redirección
- **Entonces** el sistema resta Y puntos (Penalización Menor) al usuario en ese instante

**Escenario 3 — Sin doble penalización por redirección**
- **Dado** que el ticket ya fue redirigido una vez y penalizado
- **Cuando** se realiza una redirección adicional (ej. entre agentes de soporte)
- **Entonces** el usuario NO pierde puntos adicionales por esta redirección

**Escenario 4 — Degradación automática de nivel**
- **Dado** que el saldo del usuario baja del umbral mínimo de su nivel actual
- **Cuando** se aplica la penalización
- **Entonces** el sistema degrada automáticamente al usuario al nivel inferior y registra el cambio en su historial

---

### 📊 US-18 — Ranking Mensual de Usuarios (Leaderboard)

**Como** administrador o gerente de área,  
**Quiero** visualizar un ranking mensual con los usuarios que más puntos han ganado y su tasa de calificación,  
**Para** reconocer públicamente a quienes contribuyen positivamente al sistema.

#### 📌 Criterios de Aceptación

**Escenario 1 — Generación del Top 10 mensual**
- **Dado** que el administrador accede al reporte de ranking
- **Cuando** selecciona el mes de consulta
- **Entonces** el sistema muestra los 10 usuarios con más puntos GANADOS en ese mes (no el saldo total)

**Escenario 2 — Tasa de calificación por usuario**
- **Dado** que el reporte está activo
- **Cuando** se visualiza la lista
- **Entonces** cada usuario muestra su tasa de calificación del período: `(Tickets calificados / Tickets cerrados del mes) × 100%`

---

<a name="épica-7"></a>
# 🤖 ÉPICA 7: Automatización y Mantenimiento del Ciclo de Vida

**Objetivo:** Automatizar el mantenimiento operativo del sistema para evitar acumulación de tickets inactivos y dar a los usuarios una ventana controlada para revisar resoluciones ya aplicadas.

---

### ⏰ US-19 — Recordatorio y Auto-Cierre por Inactividad del Usuario

**Como** sistema automatizado,  
**Quiero** monitorear tickets en "Esperando Información" e enviar recordatorios y cerrarlos si el usuario no responde a tiempo,  
**Para** mantener la base de datos limpia y las métricas operativas precisas.

#### 📌 Criterios de Aceptación

**Escenario 1 — Recordatorio a las 24 horas (Fase 1)**
- **Dado** que un ticket lleva 24 horas en estado "Esperando Información" sin respuesta del usuario
- **Cuando** el proceso automático se ejecuta
- **Entonces** el sistema envía una notificación al usuario recordándole que debe responder, incluyendo enlace directo al ticket

**Escenario 2 — Auto-cierre a las 48 horas (Fase 2)**
- **Dado** que el ticket lleva 48 horas en "Esperando Información" sin respuesta
- **Cuando** el proceso automático se ejecuta
- **Entonces** el ticket se cierra automáticamente con categoría "Cerrado - Sin respuesta del usuario", se registra la fecha de cierre y el historial muestra "Sistema" como ejecutor

**Escenario 3 — Respuesta del usuario interrumpe el proceso**
- **Dado** que el sistema ya envió el recordatorio de 24 horas
- **Cuando** el usuario responde antes de cumplirse las 48 horas
- **Entonces** el proceso de auto-cierre se cancela y el ticket vuelve a "En Proceso"

---

### 🔓 US-20 — Reapertura Controlada de Tickets Cerrados

**Como** usuario del sistema,  
**Quiero** poder reabrir un ticket cerrado dentro de los 5 días posteriores al cierre,  
**Para** reportar que la solución aplicada no resolvió completamente mi problema.

#### 📌 Criterios de Aceptación

**Escenario 1 — Botón de reapertura dentro del periodo de gracia**
- **Dado** que el ticket está en "Cerrado" y han pasado 5 días o menos desde el cierre
- **Cuando** el usuario visualiza el ticket
- **Entonces** se muestra el botón "Reabrir Ticket"

**Escenario 2 — Comentario obligatorio para reabrir**
- **Dado** que el usuario hace clic en "Reabrir Ticket"
- **Cuando** intenta confirmar sin comentario
- **Entonces** el sistema bloquea la acción: *"Debe ingresar el motivo de la reapertura"*

**Escenario 3 — Reapertura exitosa**
- **Dado** que el usuario confirma la reapertura con un comentario válido
- **Cuando** el sistema procesa la acción
- **Entonces** el ticket cambia a estado "Abierto", la fecha de cierre queda solo en historial, y el agente asignado recibe notificación

**Escenario 4 — Botón oculto fuera del periodo de gracia**
- **Dado** que han pasado más de 5 días desde el cierre
- **Cuando** el usuario visualiza el ticket
- **Entonces** el botón "Reabrir Ticket" no aparece y en su lugar se muestra "Crear ticket relacionado"

**Escenario 5 — Crear ticket relacionado**
- **Dado** que el usuario hace clic en "Crear ticket relacionado"
- **Cuando** se abre el formulario
- **Entonces** viene pre-poblado con los datos del ticket original (Departamento, Tipo de Soporte, Asunto, Descripción) y con una referencia visible al ticket de origen

---

---

<a name="épica-8"></a>
# 📊 ÉPICA 8: Dashboard Operativo e Inteligencia del Sistema

**Objetivo:** Proveer a cada perfil de usuario — solicitante, agente, coordinador y administrador — una vista centralizada, contextualizada y accionable del estado del sistema de tickets, respondiendo en todo momento el **qué** está pasando, el **cómo** está siendo atendido, el **cuándo** se resolverá y el **por qué** se comporta de esa manera. El dashboard no es una pantalla de reportes estática: es la superficie principal desde la que cada rol toma decisiones informadas sin necesidad de navegar ticket por ticket.

---

### 🗺️ US-21 — Vista General del Dashboard por Rol

**Como** cualquier usuario autenticado en el sistema (solicitante, agente, coordinador o administrador),  
**Quiero** acceder a un panel de inicio personalizado según mi rol que me muestre el estado actual relevante para mí,  
**Para** entender de un vistazo qué está pasando, qué requiere mi atención y qué tan saludable está el sistema en este momento.

#### 📌 Criterios de Aceptación

**Escenario 1 — Vista del solicitante**
- **Dado** que el usuario con rol Solicitante accede al dashboard
- **Cuando** el sistema carga su panel
- **Entonces** se muestran sus tickets activos agrupados por estado (Abierto, En Proceso, Esperando Información), el tiempo restante de cada uno según su prioridad, y un acceso directo a crear un nuevo ticket
- **Y** se muestra su nivel de reputación actual con la barra de progreso al siguiente nivel

**Escenario 2 — Vista del agente de soporte**
- **Dado** que el usuario con rol Agente accede al dashboard
- **Cuando** el sistema carga su panel
- **Entonces** se muestra su bandeja de tickets asignados ordenada por urgencia SLA (rojo → amarillo → verde), el número de tickets por estado y los tickets que llevan más de 24 horas sin actividad
- **Y** se muestra un indicador de su carga de trabajo actual (tickets abiertos vs. capacidad configurada)

**Escenario 3 — Vista del coordinador / supervisor**
- **Dado** que el usuario con rol Coordinador accede al dashboard
- **Cuando** el sistema carga su panel
- **Entonces** se muestra un resumen del estado de todos los tickets del departamento a su cargo, los tickets con SLA en rojo o incumplido, y la carga de trabajo por agente en su equipo
- **Y** se muestran las alertas activas de escalamiento pendientes de atención

**Escenario 4 — Vista del administrador**
- **Dado** que el usuario con rol Administrador accede al dashboard
- **Cuando** el sistema carga su panel
- **Entonces** se muestra una vista global del sistema: tickets activos en todos los departamentos, porcentaje de cumplimiento de SLA del día, métricas de volumen y distribución por prioridad
- **Y** se muestra un acceso directo a los módulos de configuración de catálogos y parámetros del sistema

**Escenario 5 — Datos en tiempo real**
- **Dado** que el dashboard está abierto en el navegador del usuario
- **Cuando** otro usuario realiza un cambio relevante en el sistema (nuevo ticket, cierre, reasignación)
- **Entonces** los contadores y listas del dashboard se actualizan sin necesidad de recargar la página (actualización periódica configurable, mínimo cada 60 segundos)

#### 📋 Notas adicionales
- El contenido visible en el dashboard está completamente filtrado por el rol y el alcance del usuario; un agente nunca ve tickets de otro departamento a menos que sea también coordinador.
- Los accesos directos (botones de acción rápida) deben llevar directamente al recurso relevante con un solo clic.
- El dashboard debe ser responsivo y funcionar correctamente en pantallas de resolución mínima de 1024 × 768.

---

### 📈 US-22 — Métricas Operativas del Sistema

**Como** coordinador o administrador,  
**Quiero** visualizar métricas agregadas y tendencias operativas del sistema de tickets dentro del dashboard,  
**Para** entender cómo está rindiendo el equipo de soporte, identificar cuellos de botella y fundamentar decisiones de mejora con datos reales.

#### 📌 Criterios de Aceptación

**Escenario 1 — Indicadores clave de rendimiento (KPIs)**
- **Dado** que el coordinador o administrador visualiza la sección de métricas del dashboard
- **Cuando** selecciona el rango de fechas de consulta (hoy, última semana, último mes o rango personalizado)
- **Entonces** el sistema muestra los siguientes KPIs calculados para ese período:
  - **Volumen total de tickets:** total creados, total cerrados, total activos
  - **Tasa de cumplimiento de SLA:** porcentaje de tickets cerrados dentro de su fecha límite sobre el total cerrado
  - **Tiempo promedio de resolución:** promedio de horas entre la fecha de creación y la fecha de cierre
  - **Tiempo promedio de primera respuesta:** promedio de horas entre la fecha de creación y la fecha en que el agente cambió por primera vez el estado a "En Proceso"
  - **Tasa de reapertura:** porcentaje de tickets cerrados que fueron reabiertos en el período
  - **Tasa de redirección:** porcentaje de tickets que requirieron al menos una redirección antes de ser resueltos

**Escenario 2 — Distribución por estado**
- **Dado** que el dashboard está cargado con el período seleccionado
- **Cuando** el usuario observa la sección de distribución
- **Entonces** se muestra un gráfico de distribución que indica cuántos tickets hay en cada estado (Abierto, En Proceso, Esperando Información, Cerrado) y qué porcentaje representan del total activo

**Escenario 3 — Tendencia de volumen en el tiempo**
- **Dado** que el administrador selecciona un período de más de 7 días
- **Cuando** el sistema renderiza la métrica de tendencia
- **Entonces** se muestra una gráfica de línea o barras con el número de tickets creados y cerrados por día (o por semana si el período supera los 30 días), permitiendo identificar picos de demanda o períodos de baja actividad

**Escenario 4 — Desglose por prioridad**
- **Dado** que el dashboard de métricas está activo
- **Cuando** el usuario revisa el desglose de tickets
- **Entonces** puede ver la distribución de tickets activos y cerrados segmentada por nivel de prioridad (Crítica, Alta, Media, Baja), con su respectiva tasa de cumplimiento de SLA por cada nivel

**Escenario 5 — Comparativa entre períodos**
- **Dado** que el usuario quiere entender si el rendimiento mejoró
- **Cuando** activa la opción de comparación con el período anterior equivalente
- **Entonces** cada KPI muestra la variación porcentual respecto al período anterior (ej. "Tiempo promedio de resolución: 8.2h ↑ +12% vs período anterior")

#### 📋 Notas adicionales
- Todos los KPIs deben ser calculados en el servidor y entregados como datos agregados; el cliente no realiza cálculos sobre conjuntos de datos completos.
- Los gráficos de tendencia deben mostrar etiquetas de valor en los puntos más altos y más bajos del período seleccionado para facilitar la lectura.
- La comparativa con el período anterior se activa con un toggle y es opcional, para no sobrecargar la vista por defecto.

---

### 👤 US-23 — Panel de Rendimiento Individual del Agente

**Como** agente de soporte (para ver su propio rendimiento) o coordinador (para ver el de su equipo),  
**Quiero** acceder a un panel de rendimiento individual que muestre las métricas de un agente específico,  
**Para** que el agente entienda su desempeño actual, identifique áreas de mejora y el coordinador pueda gestionar la carga de trabajo de su equipo de forma equitativa.

#### 📌 Criterios de Aceptación

**Escenario 1 — Vista del agente sobre su propio rendimiento**
- **Dado** que el agente accede a la sección "Mi rendimiento" dentro de su dashboard
- **Cuando** el sistema carga los datos del período actual (mes en curso por defecto)
- **Entonces** se muestra:
  - Tickets atendidos en el período (total asignados, cerrados, activos)
  - Tasa personal de cumplimiento de SLA
  - Tiempo promedio personal de resolución y de primera respuesta
  - Número de tickets que requirieron pausa (solicitud de información adicional) y promedio de tiempo de espera al usuario
  - Calificaciones promedio recibidas de los solicitantes en el período

**Escenario 2 — Vista del coordinador sobre un agente del equipo**
- **Dado** que el coordinador navega al panel de su equipo
- **Cuando** selecciona el nombre de un agente específico
- **Entonces** se muestra el mismo panel de rendimiento descrito en el Escenario 1, pero para ese agente seleccionado
- **Y** el coordinador puede ver todos los agentes de su departamento en una tabla comparativa con los mismos KPIs

**Escenario 3 — Comparativa con el promedio del equipo**
- **Dado** que el agente o coordinador visualiza el panel individual
- **Cuando** el sistema calcula las métricas
- **Entonces** cada KPI individual muestra además el promedio del equipo (departamento) para el mismo período, indicando visualmente si el agente está por encima o por debajo del promedio

**Escenario 4 — Tickets activos del agente con alerta de antigüedad**
- **Dado** que el coordinador visualiza el panel de un agente
- **Cuando** ese agente tiene tickets asignados que llevan más de 48 horas sin cambio de estado
- **Entonces** esos tickets aparecen destacados con una alerta visual de estancamiento, indicando exactamente cuántas horas llevan sin actividad registrada

**Escenario 5 — Histórico mensual del agente**
- **Dado** que el agente o coordinador consulta el historial
- **Cuando** selecciona un mes anterior en el selector de período
- **Entonces** el sistema muestra los KPIs del agente para ese mes específico, permitiendo evaluar la evolución de su rendimiento a lo largo del tiempo

#### 📋 Notas adicionales
- Un agente solo puede ver su propio panel de rendimiento. No puede ver el de sus compañeros.
- El coordinador puede ver todos los agentes de su departamento, pero no los de otros departamentos.
- El administrador puede ver todos los agentes de todos los departamentos.
- Las calificaciones promedio recibidas se calculan solo sobre tickets que el usuario solicitante efectivamente calificó; los tickets sin calificación no se incluyen en el denominador del promedio.

---

### 🗺️ US-24 — Mapa de Calor por Departamento y Tipo de Soporte

**Como** administrador o coordinador,  
**Quiero** visualizar un mapa de calor que muestre la concentración de tickets por departamento y tipo de soporte en un período determinado,  
**Para** identificar qué áreas del negocio generan más carga operativa, detectar tipos de soporte con alta tasa de incumplimiento de SLA y justificar decisiones de refuerzo de equipo o rediseño de catálogos.

#### 📌 Criterios de Aceptación

**Escenario 1 — Visualización del mapa de calor**
- **Dado** que el administrador o coordinador accede a la sección de mapa de calor
- **Cuando** selecciona el período de consulta
- **Entonces** el sistema muestra una matriz donde las filas son los Departamentos y las columnas son los Tipos de Soporte, y cada celda indica el volumen de tickets recibidos en ese cruce durante el período
- **Y** la intensidad del color de cada celda es proporcional al volumen: mayor volumen = color más intenso, menor volumen = color más claro

**Escenario 2 — Capa de SLA sobre el mapa de calor**
- **Dado** que el mapa de calor está visible
- **Cuando** el usuario activa la opción "Ver tasa de incumplimiento de SLA"
- **Entonces** el color del mapa cambia para reflejar la tasa de incumplimiento de SLA por celda: verde si la tasa es menor al 5%, amarillo entre 5% y 20%, rojo si supera el 20%

**Escenario 3 — Detalle de celda al hacer clic**
- **Dado** que el mapa de calor está visible
- **Cuando** el usuario hace clic en una celda específica (ej. Departamento: IT / Tipo de Soporte: Hardware)
- **Entonces** se despliega un panel de detalle con:
  - Total de tickets en ese cruce para el período
  - Tasa de cumplimiento de SLA
  - Tiempo promedio de resolución
  - Los 5 tickets más recientes de ese cruce con su estado actual
  - El agente con más tickets asignados en ese cruce

**Escenario 4 — Filtro por período**
- **Dado** que el administrador quiere comparar dos períodos
- **Cuando** selecciona el rango de fechas
- **Entonces** el mapa de calor se recalcula completamente para el nuevo período en menos de 5 segundos, sin recargar la página completa

**Escenario 5 — Vista del coordinador acotada a su departamento**
- **Dado** que el usuario con rol Coordinador accede al mapa de calor
- **Cuando** el sistema carga la vista
- **Entonces** solo se muestran las filas correspondientes a los departamentos bajo su responsabilidad; los demás departamentos están ocultos

#### 📋 Notas adicionales
- El mapa de calor es de solo lectura; no se realizan acciones sobre él.
- El rango máximo de consulta soportado es de 12 meses calendario.
- El sistema debe soportar hasta 20 departamentos y 50 tipos de soporte sin degradar el rendimiento visual del mapa.

---

### 📤 US-25 — Exportación de Reportes del Dashboard

**Como** coordinador o administrador,  
**Quiero** exportar los datos y visualizaciones del dashboard en formatos estándar (CSV y PDF),  
**Para** compartir reportes con gerencia, archivarlos para auditorías y utilizarlos en herramientas externas de análisis sin necesidad de acceder al sistema directamente.

#### 📌 Criterios de Aceptación

**Escenario 1 — Exportación a CSV de métricas operativas**
- **Dado** que el usuario está visualizando la sección de métricas operativas (US-22) con un período seleccionado
- **Cuando** hace clic en "Exportar CSV"
- **Entonces** el sistema genera y descarga un archivo CSV que incluye: todos los KPIs calculados para el período, la distribución por estado, la distribución por prioridad y la tendencia diaria de volumen
- **Y** el nombre del archivo incluye el período de consulta (ej. `metricas_2026-04.csv`)

**Escenario 2 — Exportación a PDF del dashboard completo**
- **Dado** que el administrador o coordinador está en cualquier vista del dashboard
- **Cuando** selecciona "Exportar PDF"
- **Entonces** el sistema genera un reporte PDF que incluye: el período de consulta en el encabezado, los KPIs principales, los gráficos de tendencia y distribución renderizados como imágenes, y la tabla comparativa del equipo si aplica al rol
- **Y** el PDF incluye en el pie de página la fecha y hora de generación, el nombre del usuario que lo generó y el nombre del sistema

**Escenario 3 — Exportación del mapa de calor**
- **Dado** que el mapa de calor (US-24) está visible con un período y capa seleccionados
- **Cuando** el usuario exporta el reporte
- **Entonces** el CSV incluye la matriz completa (departamento × tipo de soporte) con los valores de volumen y tasa de SLA de cada celda

**Escenario 4 — Exportación del rendimiento de agentes**
- **Dado** que el coordinador está visualizando la tabla comparativa de su equipo (US-23)
- **Cuando** exporta a CSV
- **Entonces** el archivo incluye una fila por agente con todos sus KPIs del período seleccionado

**Escenario 5 — Validación de exportación vacía**
- **Dado** que el usuario selecciona un período para el que no existen datos
- **Cuando** intenta exportar
- **Entonces** el sistema muestra el mensaje: *"No hay datos para el período seleccionado. Ajuste el rango de fechas e intente nuevamente"* y no genera ningún archivo

#### 📋 Notas adicionales
- Los archivos CSV deben usar codificación UTF-8 con BOM para compatibilidad con Microsoft Excel.
- El PDF debe generarse en el servidor, no en el navegador, para garantizar consistencia en el renderizado de gráficos.
- El tiempo máximo de generación del PDF es de 30 segundos; si se supera, el sistema notifica al usuario y envía el archivo por correo cuando esté listo.
- Solo los roles Coordinador y Administrador tienen acceso a las funciones de exportación.

---

## 📐 Mapa de Dependencias Entre Historias

```
US-14 (Config SLA)
    └─► US-01 (Crea ticket) ──► US-02 (Adjuntos)
              │
              ├─► US-03 (Primera apertura)
              │
              └─► US-04 (En Proceso)
                    │
                    ├─► US-05 (Redirección) ──► US-17 (Penalización menor)
                    │
                    ├─► US-06 (Esperando Info)
                    │         │
                    │         └─► US-07 (Respuesta usuario + reanudación SLA)
                    │                   │
                    │                   └─► US-19 (Auto-cierre si no responde)
                    │
                    └─► US-09 (Cierre) ──────► US-17 (Penalización mayor)
                              │
                              ├─► US-10 (Calificación) ──► US-16 (Puntos)
                              │                                  │
                              └─► US-20 (Reapertura)            └─► US-15 (Panel reputación)

US-12 (Semáforo) → Depende de: US-01, US-06, US-07
US-13 (Escalamiento) → Depende de: US-01, US-06, US-07
US-11 (Historial) → Registra eventos de: TODAS las US
US-18 (Ranking) → Depende de: US-16, US-10

US-21 (Vista general dashboard)  → Consume: US-01, US-04, US-09, US-12, US-13
US-22 (Métricas operativas)      → Consume: US-01, US-04, US-06, US-09, US-13
US-23 (Panel agente individual)  → Consume: US-03, US-04, US-06, US-09, US-12
US-24 (Mapa de calor por depto.) → Consume: US-01, US-05, US-09, US-13
US-25 (Exportar reportes)        → Consume: US-21, US-22, US-23, US-24
```

---

*Documento generado para sesión de Sprint Planning — Sistema de Gestión de Tickets v3.0*
