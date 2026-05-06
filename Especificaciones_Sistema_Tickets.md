# 📋 Especificaciones Funcionales — Sistema de Gestión de Tickets de Soporte

> **Versión:** 2.0  
> **Fecha:** Abril 2026  
> **Estado:** Lista para Refinamiento

---

## 🗂️ Índice de Épicas

| Épica | Nombre | User Stories |
|-------|--------|-------------|
| E1 | Creación y Enrutamiento Inicial | US-01, US-02 |
| E2 | Gestión y Atención por el Soporte | US-03, US-04, US-05, US-12, US-13 |
| E3 | Comunicación e Iteración | US-06, US-07, US-08 |
| E4 | Cierre, Calificación y Trazabilidad | US-09, US-10, US-11 |
| E5 | Administración y Catálogos | US-14 |
| E6 | Gamificación y Reputación | US-15, US-16, US-17, US-18 |
| E7 | Automatización y Mantenimiento | US-19, US-20 |
| E8 | Dashboard Operativo e Inteligencia del Sistema | US-21, US-22, US-23, US-24, US-25 |

---

## 📐 Conceptos Generales del Sistema

### Estados del Ticket

```
Abierto → En Proceso → Esperando Información → En Proceso → Cerrado
                                                           ↓
                                                      [Reabierto] (dentro de 5 días)
```

| Estado | Descripción |
|--------|-------------|
| **Abierto** | Ticket recién creado, aún no atendido |
| **En Proceso** | El agente está trabajando activamente en él |
| **Esperando Información** | El agente requiere datos adicionales del solicitante |
| **Cerrado** | El ticket ha sido resuelto o terminado |

### Niveles de Prioridad y SLA

| Prioridad | Tiempo Límite (configurable) |
|-----------|------------------------------|
| Crítica | 1 hora |
| Alta | 4 horas |
| Media | 24 horas |
| Baja | 48 horas |

### Roles del Sistema

| Rol | Descripción |
|-----|-------------|
| **Usuario / Solicitante** | Crea tickets y hace seguimiento |
| **Agente de Soporte** | Atiende y resuelve tickets |
| **Coordinador / Supervisor** | Recibe alertas de escalamiento |
| **Administrador** | Configura catálogos y parámetros del sistema |
| **Sistema Automatizado** | Ejecuta tareas programadas sin intervención humana |

---

# ÉPICA 1: Creación y Enrutamiento Inicial

---

## US-01 — Creación de Ticket con Cálculo Automático de Fecha Límite

### Descripción
> Como **usuario del sistema**, quiero registrar un ticket seleccionando el departamento, tipo de soporte y prioridad, detallando mi solicitud, para que el equipo correspondiente atienda mi necesidad en el tiempo establecido.

### Contexto y Motivación
El punto de entrada del sistema. Cuando un usuario necesita soporte, debe poder describir su problema y el sistema debe encargarse de enrutarlo al especialista correcto y calcular automáticamente cuándo debe ser resuelto.

---

### 🧩 Flujo Principal

1. El usuario accede al formulario de creación de ticket.
2. Selecciona el **Departamento**.
3. El sistema actualiza dinámicamente las opciones de **Tipo de Soporte** disponibles para ese departamento.
4. El usuario selecciona el **Tipo de Soporte**.
5. El usuario selecciona la **Prioridad** (Crítica, Alta, Media, Baja).
6. El usuario ingresa el **Asunto** y la **Descripción** del problema.
7. El usuario guarda el ticket.
8. El sistema ejecuta en el backend:
   - Asigna estado **"Abierto"** al ticket.
   - Registra la **fecha y hora exacta de la solicitud**.
   - Consulta las horas límite configuradas para la prioridad seleccionada.
   - Calcula la **fecha límite de resolución** sumando dichas horas a la fecha de solicitud.
   - Asigna automáticamente al **agente por defecto** configurado para ese Tipo de Soporte.
9. El sistema confirma al usuario la creación exitosa y muestra el número de ticket.

---

### ✅ Criterios de Aceptación

#### Escenario 1: Carga dinámica de tipos de soporte
```
DADO QUE el usuario está en el formulario de nuevo ticket
CUANDO selecciona un Departamento
ENTONCES el campo "Tipo de Soporte" se actualiza mostrando únicamente 
          las opciones asociadas a ese departamento
Y los tipos de soporte de otros departamentos no aparecen en la lista
```

#### Escenario 2: Creación exitosa y asignación de estado
```
DADO QUE el usuario ha completado todos los campos obligatorios
CUANDO guarda el ticket
ENTONCES el ticket se crea con estado "Abierto"
Y se registra la fecha y hora exacta como fecha de solicitud
```

#### Escenario 3: Cálculo de fecha límite
```
DADO QUE el usuario selecciona Prioridad "Alta" (configurada en 4 horas)
Y la fecha/hora de solicitud es 2026-04-23 09:00:00
CUANDO el ticket se guarda
ENTONCES la fecha límite calculada debe ser 2026-04-23 13:00:00
```

#### Escenario 4: Asignación automática al agente por defecto
```
DADO QUE el Tipo de Soporte seleccionado tiene configurado un agente por defecto
CUANDO el ticket se crea exitosamente
ENTONCES el ticket queda asignado automáticamente a ese agente
Y el agente recibe una notificación de nuevo ticket asignado
```

#### Escenario 5: Tipo de soporte sin agente configurado
```
DADO QUE el Tipo de Soporte seleccionado no tiene agente por defecto configurado
CUANDO el ticket se crea exitosamente
ENTONCES el ticket queda en bandeja general sin asignar
Y el sistema notifica al coordinador del departamento para asignación manual
```

---

### 📋 Reglas de Negocio

- RN-01: Los campos Departamento, Tipo de Soporte, Prioridad, Asunto y Descripción son **obligatorios**.
- RN-02: El campo Tipo de Soporte es **dependiente** del Departamento — no se puede seleccionar sin haber elegido departamento primero.
- RN-03: La fecha límite se calcula únicamente **en horas calendario** (no horas hábiles, a menos que se especifique lo contrario en configuración).
- RN-04: Una vez creado el ticket, la **fecha de solicitud es inmutable**.
- RN-05: El agente por defecto se configura a nivel de **Tipo de Soporte**, no de Departamento.

---

## US-02 — Adjuntar Evidencia en la Creación del Ticket

### Descripción
> Como **usuario del sistema**, quiero adjuntar archivos a mi ticket durante su creación, para proporcionar contexto visual o técnico (logs, capturas de pantalla).

### Contexto y Motivación
Los usuarios frecuentemente necesitan acompañar su reporte con evidencia. Esta funcionalidad evita comunicaciones adicionales para solicitar archivos básicos.

---

### 🧩 Flujo Principal

1. En el formulario de creación de ticket, el usuario hace clic en "Adjuntar archivo".
2. Se abre el explorador de archivos del sistema operativo.
3. El usuario selecciona uno o más archivos.
4. El sistema valida cada archivo (tamaño y extensión).
5. Si pasa validación: el archivo aparece en la lista de adjuntos pendientes.
6. Si falla validación: se muestra mensaje de error específico y el archivo es rechazado.
7. Al guardar el ticket, los archivos válidos quedan vinculados al ticket.
8. En la vista de detalle del ticket, los archivos aparecen como **enlaces de descarga**.

---

### ✅ Criterios de Aceptación

#### Escenario 1: Archivo válido adjuntado correctamente
```
DADO QUE el usuario selecciona un archivo .pdf de 2 MB
CUANDO intenta adjuntarlo al ticket
ENTONCES el archivo se agrega a la lista de adjuntos
Y al guardar el ticket queda accesible como enlace de descarga
```

#### Escenario 2: Rechazo por tamaño excedido
```
DADO QUE el usuario selecciona un archivo de 15 MB
CUANDO intenta adjuntarlo
ENTONCES el sistema muestra el mensaje: "El archivo supera el límite de 10 MB permitido"
Y el archivo NO se agrega a la lista de adjuntos
```

#### Escenario 3: Rechazo por extensión bloqueada
```
DADO QUE el usuario intenta adjuntar un archivo con extensión bloqueada (ej. .exe, .bat)
CUANDO intenta adjuntarlo
ENTONCES el sistema muestra: "Tipo de archivo no permitido"
Y el archivo es rechazado
```

#### Escenario 4: Visualización de archivos en detalle del ticket
```
DADO QUE un ticket tiene archivos adjuntos
CUANDO cualquier usuario con acceso abre el detalle del ticket
ENTONCES los archivos se muestran como enlaces de descarga con nombre y tamaño visible
```

---

### 📋 Reglas de Negocio

- RN-06: Tamaño máximo por archivo: **10 MB**.
- RN-07: Las extensiones bloqueadas deben ser configurables por el administrador (ej. .exe, .bat, .sh, .js, .php).
- RN-08: Los adjuntos son **opcionales** en la creación del ticket.
- RN-09: Se permite adjuntar **múltiples archivos** en una sola creación.

---

# ÉPICA 2: Gestión y Atención por el Soporte

---

## US-03 — Registro de Primera Apertura del Ticket

### Descripción
> Como **agente de soporte**, quiero abrir el detalle de un ticket nuevo asignado a mí, para revisar el problema reportado.

### Contexto y Motivación
Saber exactamente cuándo el agente tomó conocimiento de un ticket es crítico para auditoría y para medir tiempos de respuesta.

---

### ✅ Criterios de Aceptación

#### Escenario 1: Primera apertura del ticket por el agente asignado
```
DADO QUE el ticket tiene el campo "fecha de primera apertura" vacío
Y el agente asignado abre el detalle del ticket
CUANDO el sistema carga la vista de detalle
ENTONCES se registra automáticamente la fecha y hora exacta de apertura
Y este valor queda fijo (no se sobreescribe en aperturas posteriores)
```

#### Escenario 2: Apertura posterior por el mismo agente
```
DADO QUE el ticket ya tiene registrada la fecha de primera apertura
CUANDO el agente asignado vuelve a abrir el ticket
ENTONCES el campo de fecha de primera apertura NO se modifica
```

#### Escenario 3: Apertura por otro usuario (no el asignado)
```
DADO QUE un supervisor o administrador abre el ticket (no el agente asignado)
CUANDO se carga la vista de detalle
ENTONCES el campo de fecha de primera apertura NO se registra ni modifica
```

---

### 📋 Reglas de Negocio

- RN-10: El registro de primera apertura es **automático**, sin acción explícita del agente.
- RN-11: Solo la apertura del **agente asignado** dispara el registro.
- RN-12: El campo es de **solo escritura una vez** (inmutable tras ser establecido).

---

## US-04 — Inicio Formal de Resolución del Ticket

### Descripción
> Como **agente de soporte**, quiero cambiar el estado del ticket a "En Proceso", para indicar al usuario que su solicitud está siendo trabajada.

---

### ✅ Criterios de Aceptación

#### Escenario 1: Primer cambio a "En Proceso"
```
DADO QUE el ticket está en estado "Abierto"
CUANDO el agente cambia el estado a "En Proceso"
ENTONCES el sistema registra la fecha y hora exacta como inicio de trabajo
Y se genera una entrada en el historial de auditoría con:
  - Fecha y hora del cambio
  - Usuario que realizó el cambio
  - Estado anterior: "Abierto"
  - Estado nuevo: "En Proceso"
```

#### Escenario 2: Cambio subsecuente a "En Proceso" (tras Esperando Información)
```
DADO QUE el ticket regresa al estado "En Proceso" desde "Esperando Información"
CUANDO el sistema procesa el cambio
ENTONCES la fecha de inicio de trabajo original NO se sobreescribe
Y se genera igualmente una entrada en el historial de auditoría
```

---

### 📋 Reglas de Negocio

- RN-13: La fecha de inicio de trabajo solo se registra la **primera vez** que el ticket entra a "En Proceso".
- RN-14: Todo cambio de estado genera un **registro de auditoría** sin excepción.

---

## US-05 — Redirección de Ticket Mal Categorizado

### Descripción
> Como **agente de soporte**, quiero reasignar el ticket a otro Tipo de Soporte, para enviarlo al especialista correcto cuando el usuario se equivocó de categoría.

---

### ✅ Criterios de Aceptación

#### Escenario 1: Visualización de opciones de redirección
```
DADO QUE el agente accede a la función de redirección de un ticket
CUANDO se despliega el catálogo de Tipos de Soporte disponibles
ENTONCES solo se muestran los Tipos de Soporte que pertenecen al mismo Departamento del ticket
Y los tipos de soporte de otros departamentos NO aparecen
```

#### Escenario 2: Redirección exitosa
```
DADO QUE el agente selecciona un nuevo Tipo de Soporte válido dentro del mismo departamento
CUANDO confirma la redirección
ENTONCES el ticket queda asignado al agente por defecto del nuevo Tipo de Soporte
Y se genera una entrada en el historial de auditoría
Y el nuevo agente recibe notificación de ticket asignado
```

#### Escenario 3: Penalización al usuario por redirección (ver US-17)
```
DADO QUE se confirma la redirección del ticket
CUANDO el sistema procesa el cambio
ENTONCES se aplica automáticamente la penalización de reputación al usuario creador
(Condicionado a que no haya sido penalizado antes por redirección en este mismo ticket)
```

---

### 📋 Reglas de Negocio

- RN-15: La redirección está **restringida al mismo departamento** del ticket.
- RN-16: No se puede redirigir a un Tipo de Soporte sin agente por defecto (mostrar advertencia).
- RN-17: La redirección solo puede penalizar al usuario **una vez por ticket**.

---

## US-12 — Semáforo Visual de SLA en Bandeja de Entrada

### Descripción
> Como **agente de soporte**, quiero visualizar un indicador de tiempo restante y código de colores en mi bandeja de entrada, para identificar rápidamente qué tickets están próximos a vencer.

---

### ✅ Criterios de Aceptación

#### Escenario 1: Ticket en tiempo — indicador Verde
```
DADO QUE un ticket tiene más del 25% de su tiempo SLA restante
CUANDO el agente visualiza su bandeja de entrada
ENTONCES el indicador de ese ticket muestra color VERDE
Y la columna de tiempo restante muestra el tiempo en formato legible (ej. "3h 20min")
```

#### Escenario 2: Ticket en riesgo — indicador Amarillo
```
DADO QUE un ticket tiene entre 0% y 25% de su tiempo SLA restante
Y el SLA aún no ha vencido
CUANDO el agente visualiza su bandeja
ENTONCES el indicador muestra color AMARILLO
```

#### Escenario 3: Ticket vencido — indicador Rojo
```
DADO QUE la fecha y hora actuales superan la fecha límite del ticket
Y el ticket no está en estado "Cerrado"
CUANDO el agente visualiza su bandeja
ENTONCES el indicador muestra color ROJO
Y el tiempo restante muestra valores negativos (ej. "-2h 15min")
```

---

### 📋 Reglas de Negocio

- RN-18: El cálculo del porcentaje de tiempo restante usa la fórmula:
  `% restante = (Fecha Límite - Ahora) / (Fecha Límite - Fecha Solicitud) × 100`
- RN-19: Los tickets pausados (en "Esperando Información") deben mostrar el semáforo **congelado** o en estado neutro, ya que el SLA no corre.
- RN-20: El semáforo se actualiza en **tiempo real** o al menos al recargar la bandeja.

---

## US-13 — Escalamiento Automático por Incumplimiento de SLA

### Descripción
> Como **coordinador de soporte**, quiero que el sistema detecte cuando un ticket ha superado su fecha límite sin ser resuelto, para escalar la situación a un supervisor.

---

### ✅ Criterios de Aceptación

#### Escenario 1: Detección de SLA vencido
```
DADO QUE la fecha y hora actuales superan la fecha límite de un ticket
Y el ticket NO está en estado "Cerrado"
CUANDO el sistema ejecuta la verificación de SLA
ENTONCES el ticket se marca internamente como "SLA Incumplido"
```

#### Escenario 2: Envío de notificación al supervisor
```
DADO QUE un ticket ha sido marcado como "SLA Incumplido"
CUANDO se activa el mecanismo de notificación
ENTONCES el sistema envía una alerta al supervisor del departamento correspondiente
Y la notificación incluye: número de ticket, solicitante, descripción, fecha límite, tiempo de atraso
```

#### Escenario 3: SLA pausado — no escalamiento
```
DADO QUE un ticket está en estado "Esperando Información" (SLA pausado)
CUANDO la fecha límite original se supera
ENTONCES el sistema NO marca el ticket como "SLA Incumplido"
(El tiempo de pausa se descuenta del cálculo)
```

---

### 📋 Reglas de Negocio

- RN-21: El escalamiento se verifica de forma **automática y periódica** (proceso programado).
- RN-22: La notificación debe enviarse **una sola vez** por evento de incumplimiento (no repetir en cada ciclo).
- RN-23: El supervisor notificado es el del **departamento del ticket**, no un supervisor genérico.

---

# ÉPICA 3: Comunicación e Iteración

---

## US-06 — Solicitud de Información Adicional y Pausa de SLA

### Descripción
> Como **agente de soporte**, quiero cambiar el estado a "Esperando Información" y agregar un comentario, para pausar el trabajo hasta que el usuario aclare una duda.

---

### ✅ Criterios de Aceptación

#### Escenario 1: Cambio de estado y pausa del SLA
```
DADO QUE el ticket está en estado "En Proceso"
CUANDO el agente cambia el estado a "Esperando Información" y agrega un comentario obligatorio
ENTONCES el sistema registra la fecha y hora exacta de inicio de la pausa
Y el reloj del SLA queda suspendido desde ese instante
Y el usuario recibe una notificación indicando que se requiere su intervención
```

#### Escenario 2: Comentario obligatorio al pausar
```
DADO QUE el agente intenta cambiar el estado a "Esperando Información"
CUANDO lo hace sin ingresar un comentario explicando qué información necesita
ENTONCES el sistema bloquea el cambio y muestra: "Debe agregar un comentario explicando qué información requiere"
```

---

### 📋 Reglas de Negocio

- RN-24: El comentario es **obligatorio** al pasar a "Esperando Información".
- RN-25: La fecha de inicio de pausa se registra con **precisión de segundos**.
- RN-26: Un ticket puede entrar y salir de "Esperando Información" **múltiples veces** (pausa acumulativa).

---

## US-07 — Respuesta del Usuario y Reanudación del SLA

### Descripción
> Como **usuario del sistema**, quiero responder a las preguntas del soporte añadiendo comentarios y archivos, para facilitar la resolución de mi problema.

---

### ✅ Criterios de Aceptación

#### Escenario 1: Respuesta del usuario reactiva el ticket
```
DADO QUE el ticket está en estado "Esperando Información"
CUANDO el usuario agrega un comentario (o archivo) de respuesta
ENTONCES el sistema cambia automáticamente el estado a "En Proceso"
Y se registra la fecha y hora de reanudación
Y el sistema calcula el tiempo total que el ticket estuvo pausado
```

#### Escenario 2: Recálculo de la fecha límite
```
DADO QUE el ticket estuvo pausado 3 horas y 30 minutos
CUANDO el usuario responde y el estado regresa a "En Proceso"
ENTONCES la fecha límite se extiende en exactamente 3 horas y 30 minutos
EJEMPLO: Fecha límite original 10:00 → nueva fecha límite 13:30
```

#### Escenario 3: Ordenamiento cronológico de comentarios
```
DADO QUE un ticket tiene múltiples comentarios de distintas fechas
CUANDO un usuario o agente visualiza el hilo del ticket
ENTONCES los comentarios se muestran en orden cronológico ascendente (el más antiguo primero)
```

---

### 📋 Reglas de Negocio

- RN-27: Solo el **usuario solicitante** puede reactivar el ticket desde "Esperando Información" mediante su respuesta.
- RN-28: El tiempo de pausa acumulado es la **suma de todas las pausas** del ticket.
- RN-29: La nueva fecha límite = Fecha límite anterior + tiempo total pausado.

---

## US-08 — Notas Internas de Soporte

### Descripción
> Como **agente de soporte**, quiero agregar comentarios marcados como "Nota Interna", para documentar hallazgos técnicos que el usuario no necesita ver.

---

### ✅ Criterios de Aceptación

#### Escenario 1: Creación de nota interna
```
DADO QUE el agente está en la vista de detalle de un ticket
CUANDO selecciona la opción "Nota Interna" y escribe su comentario
ENTONCES el comentario se guarda con la marca de visibilidad restringida
Y se muestra con un color o estilo visual diferenciado en el hilo de conversación
```

#### Escenario 2: Ocultamiento al usuario solicitante
```
DADO QUE un ticket tiene notas internas
CUANDO el usuario solicitante accede a la vista del ticket
ENTONCES las notas internas NO aparecen en su hilo de conversación
Y el usuario no tiene forma de saber que existen dichas notas
```

#### Escenario 3: Visibilidad para agentes y administradores
```
DADO QUE un ticket tiene notas internas
CUANDO un agente o administrador accede al detalle del ticket
ENTONCES las notas internas SÍ son visibles, diferenciadas visualmente del resto del hilo
```

---

### 📋 Reglas de Negocio

- RN-30: Las notas internas son visibles únicamente para roles de **Agente, Coordinador y Administrador**.
- RN-31: Las notas internas siguen el mismo ordenamiento cronológico del hilo general.
- RN-32: Una nota interna **no puede convertirse** en pública una vez creada (ni viceversa).

---

# ÉPICA 4: Cierre, Calificación y Trazabilidad

---

## US-09 — Cierre Formal del Ticket

### Descripción
> Como **agente de soporte**, quiero cambiar el estado a "Cerrado" y seleccionar una categoría de resolución, para finalizar el flujo de trabajo.

---

### ✅ Criterios de Aceptación

#### Escenario 1: Cierre exitoso con categoría de resolución
```
DADO QUE el ticket está en cualquier estado activo
CUANDO el agente selecciona la categoría de resolución y confirma el cierre
ENTONCES el estado cambia a "Cerrado"
Y se registra la fecha y hora exacta de cierre
Y el usuario recibe una notificación de cierre con la categoría de resolución indicada
```

#### Escenario 2: Intento de cierre sin categoría de resolución
```
DADO QUE el agente intenta cerrar el ticket
CUANDO no ha seleccionado ninguna categoría del catálogo de resolución
ENTONCES el sistema bloquea la acción y muestra: "Debe seleccionar una categoría de resolución antes de cerrar el ticket"
```

#### Escenario 3: Categorías de resolución disponibles
```
Las opciones del catálogo de resolución incluyen (mínimo):
- ✅ Resuelto
- ❌ Rechazado
- 🔁 Duplicado
- 🚫 Cerrado - Sin respuesta del usuario (reservado para cierre automático)
```

---

### 📋 Reglas de Negocio

- RN-33: La categoría de resolución es **obligatoria** para cerrar manualmente un ticket.
- RN-34: Las categorías "Rechazado" y "Duplicado" disparan penalización de reputación al usuario (ver US-17).
- RN-35: Tras el cierre, el usuario tiene **5 días calendario** para reabrir el ticket (ver US-20).

---

## US-10 — Calificación del Servicio por el Usuario

### Descripción
> Como **usuario del sistema**, quiero calificar un ticket recién cerrado y dejar un comentario final, para evaluar la atención del agente.

---

### ✅ Criterios de Aceptación

#### Escenario 1: Calificación disponible solo en tickets cerrados
```
DADO QUE el usuario visualiza un ticket en estado "Cerrado"
CUANDO accede a la opción de calificar
ENTONCES puede seleccionar una calificación y opcionalmente agregar un comentario
```

#### Escenario 2: Calificación no disponible en tickets activos
```
DADO QUE el usuario visualiza un ticket en cualquier estado diferente a "Cerrado"
CUANDO accede al detalle del ticket
ENTONCES la opción de calificación NO es visible
```

#### Escenario 3: Una sola calificación por ticket
```
DADO QUE el usuario ya calificó un ticket cerrado
CUANDO intenta volver a calificarlo
ENTONCES el sistema muestra su calificación anterior en modo de solo lectura
Y no permite modificarla
```

---

### 📋 Reglas de Negocio

- RN-36: La calificación es **voluntaria** pero genera puntos de reputación al realizarla (ver US-16).
- RN-37: El comentario adicional en la calificación es **opcional** pero otorga puntos extra.
- RN-38: Solo el **usuario solicitante** del ticket puede calificarlo.

---

## US-11 — Historial de Auditoría y Trazabilidad

### Descripción
> Como **administrador o auditor**, quiero ver una pestaña de "Historial" en el detalle del ticket, para auditar quién hizo qué y cuándo a lo largo del ciclo de vida.

---

### ✅ Criterios de Aceptación

#### Escenario 1: Visualización del historial completo
```
DADO QUE el administrador abre el detalle de un ticket
CUANDO accede a la pestaña "Historial"
ENTONCES se muestra una tabla con todas las acciones registradas, conteniendo:
  - Fecha y hora de la acción
  - Usuario que realizó la acción (o "Sistema" si fue automático)
  - Tipo de acción (ej. "Cambio de Estado", "Comentario agregado", "Archivo adjunto")
  - Valor anterior (ej. "Abierto")
  - Valor nuevo (ej. "En Proceso")
```

#### Escenario 2: Registro de acciones automatizadas
```
DADO QUE el sistema realiza una acción automática (ej. auto-cierre, escalamiento)
CUANDO se registra en el historial
ENTONCES el campo "Usuario" muestra "Sistema" en lugar de un nombre de persona
```

#### Escenario 3: Ordenamiento del historial
```
DADO QUE el ticket tiene múltiples entradas en su historial
CUANDO el auditor visualiza la pestaña
ENTONCES las entradas se muestran en orden cronológico descendente (la más reciente primero)
```

---

### 📋 Reglas de Negocio

- RN-39: El historial es de **solo lectura** — no se puede editar ni eliminar entradas.
- RN-40: **Todo** cambio de estado, reasignación, comentario y acción sistémica debe generar un registro.
- RN-41: El historial es accesible para roles de **Agente, Coordinador y Administrador** (no para el usuario solicitante en vista básica).

---

# ÉPICA 5: Administración y Catálogos

---

## US-14 — Configuración de Tiempos SLA por Prioridad

### Descripción
> Como **administrador del sistema**, quiero configurar el tiempo máximo de resolución para cada nivel de prioridad, para definir los acuerdos de nivel de servicio sin modificar código.

---

### ✅ Criterios de Aceptación

#### Escenario 1: Edición de tiempo SLA de una prioridad
```
DADO QUE el administrador accede al panel de configuración de prioridades
CUANDO modifica el tiempo límite de "Prioridad Media" de 24h a 16h y guarda
ENTONCES el nuevo valor queda registrado en el sistema
Y todos los tickets creados A PARTIR de ese momento usarán el valor 16h
```

#### Escenario 2: No retroactividad de los cambios
```
DADO QUE existe un ticket creado antes del cambio de configuración
CUANDO el administrador modifica el tiempo SLA de su prioridad
ENTONCES la fecha límite del ticket existente NO cambia
Y el ticket mantiene su fecha límite original
```

#### Escenario 3: Validación de valores permitidos
```
DADO QUE el administrador ingresa un valor de horas
CUANDO el valor ingresado es 0, negativo o no es un número entero
ENTONCES el sistema rechaza el cambio y muestra un mensaje de validación
```

---

### 📋 Reglas de Negocio

- RN-42: El campo de horas acepta únicamente **números enteros positivos**.
- RN-43: Los cambios aplican **prospectivamente**, nunca retroactivamente.
- RN-44: Debe existir historial de cambios de configuración con fecha y administrador que realizó el cambio.

---

# ÉPICA 6: Gamificación y Reputación del Solicitante

---

## US-15 — Visualización del Panel de Reputación del Usuario

### Descripción
> Como **usuario del sistema**, quiero ver mi nivel de reputación actual y mis puntos acumulados en mi panel principal, para conocer mi prestigio dentro de la plataforma.

---

### ✅ Criterios de Aceptación

#### Escenario 1: Visualización del nivel y puntos
```
DADO QUE el usuario accede a su panel principal
CUANDO el sistema carga su perfil de reputación
ENTONCES se muestra:
  - Ícono o medalla del nivel actual (ej. 🥇 Oro, 🥈 Plata, 🥉 Bronce)
  - Puntos actuales acumulados
  - Barra de progreso indicando cuántos puntos faltan para el siguiente nivel
```

#### Escenario 2: Acceso al historial de puntos
```
DADO QUE el usuario hace clic en su indicador de reputación
CUANDO se abre el detalle de puntos
ENTONCES se muestra una tabla con:
  - Fecha de la transacción
  - Motivo (ej. "Calificación enviada", "Ticket rechazado")
  - Puntos ganados o perdidos (positivo / negativo)
  - Saldo acumulado
```

---

### 📋 Reglas de Negocio

- RN-45: Los niveles de reputación y sus umbrales son configurables por el administrador.
- RN-46: La barra de progreso solo muestra el **nivel siguiente**, no niveles futuros.

---

## US-16 — Otorgamiento de Puntos por Calificación

### Descripción
> Como **sistema automatizado**, quiero otorgar puntos de reputación al usuario cuando califica un ticket cerrado, para incentivar la retroalimentación.

---

### ✅ Criterios de Aceptación

#### Escenario 1: Puntos base por calificación
```
DADO QUE el usuario envía una calificación de un ticket cerrado
CUANDO el sistema procesa la calificación
ENTONCES se suman X puntos al saldo del usuario
Y se registra la transacción en el historial de puntos con motivo "Calificación enviada"
```

#### Escenario 2: Bono por comentario adicional
```
DADO QUE el usuario envía una calificación E incluye texto en el campo de comentarios adicionales
CUANDO el sistema procesa la calificación
ENTONCES se suman X puntos base + Y puntos adicionales de bono
Y la transacción registra ambos conceptos de forma separada
```

#### Escenario 3: Sin puntos por calificación repetida
```
DADO QUE el usuario ya calificó un ticket
CUANDO intenta volver a calificar (acción que el sistema bloqueará según US-10)
ENTONCES NO se generan puntos adicionales
```

---

### 📋 Reglas de Negocio

- RN-47: Los valores de X (puntos base) e Y (bono) son **configurables por el administrador**.
- RN-48: Solo se otorgan puntos **una vez por ticket calificado**.

---

## US-17 — Penalización por Tickets Inválidos o Mal Categorizados

### Descripción
> Como **sistema automatizado**, quiero restar puntos de reputación al usuario cuando su ticket es cerrado con resolución negativa o requiere redirección, para desincentivar el mal uso del sistema.

---

### ✅ Criterios de Aceptación

#### Escenario 1: Penalización mayor por cierre con resolución negativa
```
DADO QUE un agente cierra un ticket seleccionando "Rechazado" o "Duplicado"
CUANDO el sistema procesa el cierre
ENTONCES se restan X puntos (Penalización Mayor) al usuario solicitante
Y se registra en el historial de puntos con motivo específico (ej. "Penalización: Ticket duplicado")
```

#### Escenario 2: Penalización menor por redirección
```
DADO QUE un agente utiliza la función de redirección en un ticket
Y es la primera redirección de ese ticket
CUANDO el sistema procesa la redirección
ENTONCES se restan Y puntos (Penalización Menor) al usuario creador en ese instante
Y se registra en el historial de puntos con motivo "Penalización: Ticket redirigido por soporte"
```

#### Escenario 3: Una sola penalización por redirección por ticket
```
DADO QUE un ticket ya fue redirigido una vez (penalización aplicada)
CUANDO el ticket es redirigido nuevamente (por error del equipo de soporte)
ENTONCES NO se aplica nueva penalización al usuario
Y el sistema ignora el evento de penalización por redirección subsecuente
```

#### Escenario 4: Degradación de nivel por penalización
```
DADO QUE el usuario tiene 100 puntos y está en nivel "Plata" (umbral mínimo: 80 puntos)
CUANDO recibe una penalización que baja su saldo a 75 puntos
ENTONCES el sistema degrada automáticamente al usuario al nivel "Bronce"
Y se registra el cambio de nivel en el historial de puntos
```

---

### 📋 Reglas de Negocio

- RN-49: Los valores de penalización X e Y son **configurables por el administrador**.
- RN-50: La penalización por redirección aplica **en el momento** de la redirección, no al cierre.
- RN-51: La degradación de nivel es **inmediata** al cruzar el umbral inferior.
- RN-52: El saldo de puntos **no puede ser negativo** (mínimo: 0).

---

## US-18 — Ranking Mensual (Leaderboard)

### Descripción
> Como **administrador o gerente de área**, quiero visualizar un ranking mensual de usuarios con más puntos ganados, para reconocer a quienes más contribuyen.

---

### ✅ Criterios de Aceptación

#### Escenario 1: Generación del Top 10 mensual
```
DADO QUE el administrador accede al reporte de ranking
CUANDO selecciona el mes en curso
ENTONCES el sistema muestra los 10 usuarios con más puntos GANADOS en ese mes
(Solo puntos positivos generados en el período, no el saldo total acumulado)
```

#### Escenario 2: Tasa de calificación por usuario
```
DADO QUE el reporte está visible
CUANDO el administrador revisa cada usuario del ranking
ENTONCES se muestra la "Tasa de Calificación":
  = (Tickets calificados por el usuario en el mes) / (Tickets cerrados del usuario en el mes) × 100%
```

#### Escenario 3: Filtrado por mes
```
DADO QUE el administrador selecciona un mes histórico
CUANDO el sistema genera el reporte
ENTONCES muestra los datos correspondientes a ese mes, no al mes actual
```

---

### 📋 Reglas de Negocio

- RN-53: El ranking considera únicamente los **puntos ganados** (transacciones positivas) en el mes.
- RN-54: La tasa de calificación considera solo tickets del **mismo período mensual**.

---

# ÉPICA 7: Automatización y Mantenimiento del Ciclo de Vida

---

## US-19 — Recordatorio y Auto-Cierre por Inactividad del Usuario

### Descripción
> Como **sistema automatizado**, quiero monitorear los tickets en "Esperando Información" para enviar recordatorios y cerrarlos automáticamente si el tiempo expira, para mantener la higiene operativa.

---

### ✅ Criterios de Aceptación

#### Escenario 1: Envío de recordatorio (Fase 1 — 24 horas)
```
DADO QUE un ticket está en estado "Esperando Información"
Y han transcurrido exactamente 24 horas desde el último cambio de estado
Y el usuario NO ha agregado ninguna respuesta
CUANDO el proceso automático se ejecuta
ENTONCES el sistema envía una notificación al usuario recordándole que debe responder
Y la notificación incluye un enlace directo al ticket
```

#### Escenario 2: Auto-cierre (Fase 2 — 48 horas)
```
DADO QUE un ticket está en "Esperando Información"
Y han transcurrido 48 horas desde el cambio de estado sin respuesta del usuario
CUANDO el proceso automático se ejecuta
ENTONCES el sistema cierra automáticamente el ticket
Y asigna la categoría de resolución "Cerrado - Sin respuesta del usuario"
Y registra la fecha y hora de cierre
Y el campo de usuario en el historial muestra "Sistema"
```

#### Escenario 3: Respuesta del usuario interrumpe el proceso
```
DADO QUE el sistema ya envió el recordatorio de 24 horas
CUANDO el usuario responde antes de que se cumplan las 48 horas
ENTONCES el sistema cancela el proceso de auto-cierre
Y el ticket regresa a estado "En Proceso" (según US-07)
```

---

### 📋 Reglas de Negocio

- RN-55: El proceso de monitoreo se ejecuta **una vez al día** (configurable).
- RN-56: El recordatorio se envía **una sola vez** por período de inactividad.
- RN-57: El auto-cierre del sistema **no genera penalización de reputación** al usuario.

---

## US-20 — Reapertura Controlada de Tickets Cerrados

### Descripción
> Como **usuario del sistema**, quiero reabrir un ticket cerrado dentro de un periodo de gracia, para reportar si la solución aplicada no resolvió mi problema.

---

### ✅ Criterios de Aceptación

#### Escenario 1: Reapertura dentro del periodo de gracia (≤ 5 días)
```
DADO QUE un ticket está en estado "Cerrado"
Y la diferencia entre la fecha actual y la fecha de cierre es de 5 días o menos
CUANDO el usuario visualiza el ticket
ENTONCES se muestra el botón "Reabrir Ticket"
```

#### Escenario 2: Reapertura con comentario obligatorio
```
DADO QUE el usuario hace clic en "Reabrir Ticket"
CUANDO intenta confirmar la reapertura sin ingresar un comentario de justificación
ENTONCES el sistema bloquea la acción con el mensaje: "Debe ingresar el motivo de la reapertura"
```

#### Escenario 3: Reapertura exitosa
```
DADO QUE el usuario confirma la reapertura con un comentario válido
CUANDO el sistema procesa la acción
ENTONCES el ticket cambia al estado "Abierto" (o "Reabierto")
Y la fecha de cierre anterior queda registrada en historial pero se limpia del campo activo
Y el agente asignado recibe una notificación de reapertura
```

#### Escenario 4: Bloqueo histórico — botón oculto después de 5 días
```
DADO QUE un ticket está en estado "Cerrado"
Y han pasado más de 5 días desde la fecha de cierre
CUANDO el usuario visualiza el ticket
ENTONCES el botón "Reabrir Ticket" NO aparece
Y en su lugar se muestra el botón "Crear ticket relacionado"
```

#### Escenario 5: Crear ticket relacionado desde un ticket expirado
```
DADO QUE el usuario hace clic en "Crear ticket relacionado"
CUANDO se abre el formulario de nuevo ticket
ENTONCES viene pre-poblado con: Departamento, Tipo de Soporte, Asunto y Descripción del ticket original
Y incluye una referencia visible al ticket de origen (ej. "Derivado del ticket #1234")
```

---

### 📋 Reglas de Negocio

- RN-58: El periodo de gracia es de **5 días calendario** (no hábiles).
- RN-59: La reapertura elimina la fecha de cierre del campo activo pero la **preserva en el historial**.
- RN-60: Un ticket puede ser reabierto **múltiples veces** dentro de los 5 días posteriores a cada cierre.
- RN-61: El ticket relacionado lleva una **referencia bidireccional** con el ticket original.

---

---

# ÉPICA 8: Dashboard Operativo e Inteligencia del Sistema

---

## US-21 — Vista General del Dashboard por Rol

### Descripción
> Como **cualquier usuario autenticado**, quiero un panel de inicio personalizado según mi rol que muestre el estado actual relevante para mí, para entender de un vistazo qué está pasando y qué requiere mi atención.

### Contexto y Motivación
Cada perfil tiene necesidades de información distintas. El solicitante quiere saber cómo van sus tickets. El agente necesita saber qué atender primero. El coordinador necesita ver la salud de su equipo. El administrador requiere una visión global del sistema. Un único dashboard parametrizado por rol satisface las cuatro necesidades sin crear cuatro pantallas separadas.

---

### 🧩 Flujo Principal

1. El usuario se autentica y es redirigido al dashboard correspondiente a su rol.
2. El sistema determina el alcance del usuario (departamentos, tickets asignados, equipos bajo supervisión).
3. El sistema consulta y agrega los datos relevantes para ese rol y alcance.
4. El dashboard se renderiza mostrando los módulos, contadores, listas y accesos directos correspondientes al rol.
5. La información se refresca automáticamente cada 60 segundos sin recargar la página.

---

### ✅ Criterios de Aceptación

#### Escenario 1: Vista del solicitante
```
DADO QUE el usuario con rol Solicitante accede al sistema
CUANDO el dashboard se carga
ENTONCES se muestran sus tickets activos agrupados por estado
Y se muestra su nivel de reputación y barra de progreso al siguiente nivel
Y hay un acceso directo a crear un nuevo ticket
```

#### Escenario 2: Vista del agente
```
DADO QUE el usuario con rol Agente accede al sistema
CUANDO el dashboard se carga
ENTONCES se muestra su bandeja ordenada por urgencia de SLA (rojo → amarillo → verde)
Y se muestran los tickets sin actividad en las últimas 24 horas con alerta visual
Y se muestra su indicador de carga de trabajo actual
```

#### Escenario 3: Vista del coordinador
```
DADO QUE el usuario con rol Coordinador accede al sistema
CUANDO el dashboard se carga
ENTONCES se muestra el resumen de tickets del departamento a su cargo
Y se muestran los tickets con SLA incumplido o en rojo
Y se muestra la carga de trabajo por agente de su equipo
Y se listan las alertas de escalamiento activas pendientes de atención
```

#### Escenario 4: Vista del administrador
```
DADO QUE el usuario con rol Administrador accede al sistema
CUANDO el dashboard se carga
ENTONCES se muestra una vista global: tickets activos en todos los departamentos,
  porcentaje de cumplimiento de SLA del día y distribución por prioridad
Y hay accesos directos a módulos de configuración de catálogos
```

#### Escenario 5: Actualización en tiempo cuasi-real
```
DADO QUE el dashboard está abierto
CUANDO transcurren 60 segundos o un evento relevante ocurre en el sistema
ENTONCES los contadores y listas se actualizan sin recargar la página
```

---

### 📋 Reglas de Negocio

- RN-62: El contenido del dashboard está **filtrado por el rol y el alcance** del usuario autenticado; nunca se exponen datos fuera de su jurisdicción.
- RN-63: El intervalo de refresco automático es **configurable por el administrador** (mínimo 30 segundos, máximo 5 minutos).
- RN-64: El dashboard debe ser funcional en resoluciones desde **1024 × 768 píxeles**.
- RN-65: Los accesos directos llevan al recurso relevante **en un solo clic** sin pasos intermedios.

---

## US-22 — Métricas Operativas del Sistema

### Descripción
> Como **coordinador o administrador**, quiero visualizar métricas agregadas y tendencias operativas dentro del dashboard, para entender cómo rinde el equipo, identificar cuellos de botella y tomar decisiones fundamentadas en datos.

---

### ✅ Criterios de Aceptación

#### Escenario 1: KPIs del período seleccionado
```
DADO QUE el coordinador o administrador accede a la sección de métricas
CUANDO selecciona un rango de fechas (hoy, última semana, último mes o rango personalizado)
ENTONCES el sistema muestra los siguientes indicadores calculados:
  - Volumen: tickets creados, cerrados y activos
  - Tasa de cumplimiento de SLA: % cerrados a tiempo sobre total cerrado
  - Tiempo promedio de resolución: horas entre creación y cierre
  - Tiempo promedio de primera respuesta: horas hasta primer cambio a "En Proceso"
  - Tasa de reapertura: % de tickets cerrados que fueron reabiertos
  - Tasa de redirección: % de tickets que requirieron al menos una redirección
```

#### Escenario 2: Distribución por estado
```
DADO QUE el dashboard está cargado
CUANDO el usuario observa la sección de distribución
ENTONCES se muestra cuántos tickets hay en cada estado
Y cada estado indica qué porcentaje representa del total activo
```

#### Escenario 3: Tendencia de volumen en el tiempo
```
DADO QUE el período seleccionado es mayor a 7 días
CUANDO el sistema renderiza la tendencia
ENTONCES se muestra una gráfica de tickets creados y cerrados por día
Y si el período supera 30 días, la granularidad cambia a semana
```

#### Escenario 4: Desglose por prioridad
```
DADO QUE el dashboard de métricas está activo
CUANDO el usuario revisa la distribución por prioridad
ENTONCES ve tickets activos y cerrados segmentados por nivel (Crítica, Alta, Media, Baja)
Y ve la tasa de cumplimiento de SLA específica de cada nivel
```

#### Escenario 5: Comparativa con período anterior
```
DADO QUE el usuario activa el toggle de comparación
CUANDO el sistema recalcula los datos
ENTONCES cada KPI muestra la variación porcentual vs. el período anterior equivalente
EJEMPLO: "Tiempo promedio resolución: 8.2h ↑ +12% vs período anterior"
```

---

### 📋 Reglas de Negocio

- RN-66: Los KPIs se calculan **en el servidor**; el cliente no procesa conjuntos de datos completos.
- RN-67: La comparativa con el período anterior se activa con un **toggle opcional** para no sobrecargar la vista por defecto.
- RN-68: Los gráficos de tendencia deben etiquetar visualmente el **punto más alto y el más bajo** del período.

---

## US-23 — Panel de Rendimiento Individual del Agente

### Descripción
> Como **agente** (para ver su propio rendimiento) o **coordinador** (para ver el de su equipo), quiero acceder a un panel de métricas individuales por agente, para evaluar desempeño, compararlo con el equipo e identificar tickets estancados.

---

### ✅ Criterios de Aceptación

#### Escenario 1: Vista del agente sobre sí mismo
```
DADO QUE el agente accede a "Mi rendimiento" en su dashboard
CUANDO el sistema carga el período actual (mes en curso por defecto)
ENTONCES se muestra:
  - Tickets atendidos: asignados, cerrados, activos en el período
  - Tasa personal de cumplimiento de SLA
  - Tiempo promedio personal de resolución y primera respuesta
  - Número de pausas solicitadas (solicitudes de información) y tiempo promedio de espera
  - Calificación promedio recibida de los solicitantes en el período
```

#### Escenario 2: Vista del coordinador sobre un agente
```
DADO QUE el coordinador navega al panel de su equipo
CUANDO selecciona un agente específico
ENTONCES se muestran las mismas métricas del Escenario 1 para ese agente
Y puede ver una tabla comparativa de todos los agentes del departamento
```

#### Escenario 3: Comparativa con el promedio del equipo
```
DADO QUE el panel individual está visible
CUANDO el sistema calcula las métricas
ENTONCES cada KPI muestra también el promedio del equipo para el mismo período
Y una señal visual indica si el agente está por encima o por debajo del promedio
```

#### Escenario 4: Alerta de tickets estancados
```
DADO QUE el coordinador visualiza el panel de un agente
CUANDO ese agente tiene tickets sin cambio de estado en más de 48 horas
ENTONCES esos tickets aparecen destacados con el número exacto de horas sin actividad
```

#### Escenario 5: Histórico mensual del agente
```
DADO QUE el usuario selecciona un mes anterior en el selector
CUANDO el sistema responde
ENTONCES se muestran los KPIs del agente para ese mes específico
```

---

### 📋 Reglas de Negocio

- RN-69: Un agente **solo puede ver su propio** panel; no puede consultar el de sus compañeros.
- RN-70: El coordinador puede ver **todos los agentes de su departamento**, pero no de otros.
- RN-71: Las calificaciones promedio excluyen tickets **sin calificación** del denominador; solo se promedian los efectivamente calificados.
- RN-72: Un ticket se considera "estancado" si lleva más de **48 horas consecutivas** sin ningún cambio de estado, comentario ni acción registrada en su historial.

---

## US-24 — Mapa de Calor por Departamento y Tipo de Soporte

### Descripción
> Como **administrador o coordinador**, quiero visualizar un mapa de calor que cruce departamentos contra tipos de soporte mostrando volumen e incumplimiento de SLA, para identificar zonas de alta carga y tipos de soporte problemáticos.

---

### ✅ Criterios de Aceptación

#### Escenario 1: Visualización del mapa de calor de volumen
```
DADO QUE el administrador accede a la sección de mapa de calor
CUANDO selecciona el período de consulta
ENTONCES se muestra una matriz: filas = Departamentos, columnas = Tipos de Soporte
Y cada celda muestra el volumen de tickets de ese cruce en el período
Y la intensidad del color de la celda es proporcional al volumen (mayor volumen = más intenso)
```

#### Escenario 2: Capa de tasa de incumplimiento de SLA
```
DADO QUE el mapa de calor de volumen está visible
CUANDO el usuario activa "Ver tasa de incumplimiento de SLA"
ENTONCES el color del mapa refleja la tasa de incumplimiento:
  Verde: tasa < 5% | Amarillo: entre 5% y 20% | Rojo: > 20%
```

#### Escenario 3: Panel de detalle al hacer clic en una celda
```
DADO QUE el mapa está visible
CUANDO el usuario hace clic en una celda (ej. IT / Hardware)
ENTONCES se despliega un panel con:
  - Total de tickets del cruce en el período
  - Tasa de cumplimiento de SLA
  - Tiempo promedio de resolución
  - Los 5 tickets más recientes con su estado actual
  - El agente con más tickets asignados en ese cruce
```

#### Escenario 4: Recálculo dinámico al cambiar período
```
DADO QUE el usuario cambia el rango de fechas
CUANDO el sistema procesa el nuevo rango
ENTONCES el mapa se recalcula y re-renderiza en menos de 5 segundos
```

#### Escenario 5: Vista acotada para coordinador
```
DADO QUE el usuario es Coordinador
CUANDO accede al mapa de calor
ENTONCES solo se muestran las filas de los departamentos bajo su responsabilidad
```

---

### 📋 Reglas de Negocio

- RN-73: El mapa de calor es de **solo lectura**; no se realizan acciones sobre él.
- RN-74: El rango máximo de consulta es de **12 meses calendario**.
- RN-75: El sistema debe soportar hasta **20 departamentos × 50 tipos de soporte** sin degradar el rendimiento visual.

---

## US-25 — Exportación de Reportes del Dashboard

### Descripción
> Como **coordinador o administrador**, quiero exportar los datos del dashboard en CSV y PDF, para compartir reportes con gerencia, archivarlos y analizarlos en herramientas externas.

---

### ✅ Criterios de Aceptación

#### Escenario 1: Exportación CSV de métricas operativas
```
DADO QUE el usuario visualiza la sección de métricas con un período seleccionado
CUANDO hace clic en "Exportar CSV"
ENTONCES se descarga un archivo CSV con todos los KPIs, distribuciones y tendencia diaria
Y el nombre del archivo incluye el período (ej. metricas_2026-04.csv)
```

#### Escenario 2: Exportación PDF del dashboard completo
```
DADO QUE el usuario está en cualquier vista del dashboard
CUANDO selecciona "Exportar PDF"
ENTONCES se genera un PDF con: período en el encabezado, KPIs, gráficos como imágenes,
  tabla del equipo si aplica
Y el pie de página incluye: fecha de generación, nombre del usuario y nombre del sistema
```

#### Escenario 3: Exportación del mapa de calor
```
DADO QUE el mapa de calor está visible con período y capa seleccionados
CUANDO el usuario exporta
ENTONCES el CSV incluye la matriz completa con volumen y tasa de SLA por celda
```

#### Escenario 4: Exportación del rendimiento de agentes
```
DADO QUE el coordinador visualiza la tabla comparativa de su equipo
CUANDO exporta a CSV
ENTONCES el archivo incluye una fila por agente con todos sus KPIs del período
```

#### Escenario 5: Período sin datos
```
DADO QUE el usuario selecciona un período sin datos registrados
CUANDO intenta exportar
ENTONCES el sistema muestra: "No hay datos para el período seleccionado. 
  Ajuste el rango de fechas e intente nuevamente"
Y no se genera ningún archivo
```

---

### 📋 Reglas de Negocio

- RN-76: Los archivos CSV se generan en **codificación UTF-8 con BOM** para compatibilidad con Excel.
- RN-77: El PDF se genera **en el servidor**, no en el navegador, para garantizar consistencia.
- RN-78: Si la generación del PDF supera **30 segundos**, el sistema notifica al usuario y envía el archivo por correo cuando esté listo.
- RN-79: Solo los roles **Coordinador y Administrador** tienen acceso a las funciones de exportación.

---

## 📊 Matriz de Impacto entre Historias

| US | Depende de | Impacta en |
|----|-----------|------------|
| US-01 | — | US-02, US-03, US-04, US-12, US-13 |
| US-04 | US-01, US-03 | US-06, US-09 |
| US-05 | US-01 | US-17 |
| US-06 | US-04 | US-07, US-19 |
| US-07 | US-06 | SLA recalculado |
| US-09 | US-04 | US-10, US-17, US-20 |
| US-10 | US-09 | US-16 |
| US-14 | — | US-01 (solo futuros) |
| US-16 | US-10 | US-15 |
| US-17 | US-05, US-09 | US-15 |
| US-19 | US-06 | US-09 |
| US-20 | US-09 | US-01 |
| US-21 | US-01, US-04, US-09, US-12, US-13 | Dashboard base para US-22–25 |
| US-22 | US-01, US-04, US-06, US-09, US-13 | US-25 (exportación) |
| US-23 | US-03, US-04, US-06, US-09, US-12 | US-25 (exportación) |
| US-24 | US-01, US-05, US-09, US-13 | US-25 (exportación) |
| US-25 | US-21, US-22, US-23, US-24 | — |

---

*Documento generado para sesión de refinamiento de producto — Versión 2.0*
