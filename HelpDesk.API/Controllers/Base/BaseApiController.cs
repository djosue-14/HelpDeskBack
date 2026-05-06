using HelpDesk.Application.Common;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HelpDesk.API.Controllers.Base;

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
