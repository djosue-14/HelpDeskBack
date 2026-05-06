namespace HelpDesk.Application.DTOs.Department;

public class CreateDepartmentRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? CoordinatorUserId { get; set; }
}
