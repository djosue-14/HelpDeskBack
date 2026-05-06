# BaseApiController — Patrón de Controlador Base

## Propósito

`BaseApiController` centraliza el boilerplate repetitivo de todos los controladores de la API siguiendo los principios **SOLID** y **DRY**.

## Implementación Real

```csharp
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class BaseApiController : ControllerBase
{
    protected string GetCurrentUser() =>
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

    protected string GetCurrentRole() =>
        User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

    protected IActionResult HandleResult<T>(Result<T> result) =>
        result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });

    protected IActionResult HandleResult(Result result) =>
        result.IsSuccess ? Ok() : BadRequest(new { error = result.Error });

    protected IActionResult HandleGetResult<T>(Result<T> result) =>
        result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });

    protected IActionResult HandleCreateResult<T>(Result<T> result, string actionName, object routeValues) =>
        result.IsSuccess
            ? CreatedAtAction(actionName, routeValues, result.Value)
            : BadRequest(new { error = result.Error });

    protected IActionResult HandleDeleteResult(Result result) =>
        result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
}
```

## Características

### 1. Atributos heredados
- `[ApiController]` — validación automática de ModelState, binding implícito
- `[Route("api/[controller]")]` — ruta base por nombre de controlador
- `[Produces("application/json")]` — content type de respuesta

Los controladores derivados **no repiten** estos atributos. Si un controlador necesita una ruta distinta (como `TicketCommentsController`), declara su propio `[Route]` que sobreescribe el del base.

### 2. Extracción de claims del usuario autenticado

```csharp
protected string GetCurrentUser() =>
    User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

protected string GetCurrentRole() =>
    User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
```

En desarrollo se usa un middleware que inyecta claims falsos (`DevBypass`). En producción se usan los claims del token JWT real. Los controladores no saben cuál es la fuente — solo llaman `GetCurrentUser()`.

### 3. Helpers de Result Pattern

#### `HandleResult(Result result)` — void operations
- `IsSuccess` → `200 OK`
- `IsFailure` → `400 Bad Request { error }`

#### `HandleResult<T>(Result<T> result)` — operaciones con valor
- `IsSuccess` → `200 OK` con `result.Value`
- `IsFailure` → `400 Bad Request { error }`

#### `HandleGetResult<T>(Result<T> result)` — GET por ID
- `IsSuccess` → `200 OK` con `result.Value`
- `IsFailure` → `404 Not Found { error }`

#### `HandleCreateResult<T>(Result<T> result, string actionName, object routeValues)` — POST/Create
- `IsSuccess` → `201 Created` con Location header
- `IsFailure` → `400 Bad Request { error }`

#### `HandleDeleteResult(Result result)` — DELETE
- `IsSuccess` → `204 No Content`
- `IsFailure` → `400 Bad Request { error }`

## Ejemplo de Uso

### Antes (sin BaseApiController)
```csharp
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TicketsController : ControllerBase
{
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTicketRequest request, CancellationToken cancellationToken)
    {
        var currentUser = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        var result = await _service.CreateAsync(request, currentUser, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(GetById), new { id = result.Value?.TicketId }, result.Value);
    }
}
```

### Después (con BaseApiController)
```csharp
public class TicketsController : BaseApiController
{
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        return HandleGetResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTicketRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(request, GetCurrentUser(), cancellationToken);
        return HandleCreateResult(result, nameof(GetById), new { id = result.Value?.TicketId });
    }
}
```

## Reducción de Código

- **Antes**: ~12–15 líneas por endpoint
- **Después**: ~3–4 líneas por endpoint
- **Ahorro**: ~70% menos código repetitivo en 8 controladores

## Integración con Autenticación Real (JWT)

Cuando se active JWT, el único cambio requerido es:
1. Eliminar el middleware `DevBypass` en `Program.cs`
2. Agregar `builder.Services.AddAuthentication(...)` y `app.UseAuthentication()`
3. Los controladores **no cambian** — siguen usando `GetCurrentUser()` y `GetCurrentRole()`

## Caso Especial: TicketCommentsController

`TicketCommentsController` declara su propia ruta (sub-recurso de ticket):

```csharp
[Route("api/Tickets/{ticketId:int}/comments")]
public class TicketCommentsController : BaseApiController
{
    // Esta ruta sobreescribe "api/[controller]" del base
}
```

Los demás atributos (`[ApiController]`, `[Produces]`) siguen heredándose del base.
