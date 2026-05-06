namespace HelpDesk.Application.DTOs.Department;

public class DepartmentDto
{
    public int DepartmentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? CoordinatorUserId { get; set; }
    public bool IsEnabled { get; set; }
}
