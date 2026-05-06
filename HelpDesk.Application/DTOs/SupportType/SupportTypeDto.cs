namespace HelpDesk.Application.DTOs.SupportType;

public class SupportTypeDto
{
    public int SupportTypeId { get; set; }
    public int DepartmentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
}
