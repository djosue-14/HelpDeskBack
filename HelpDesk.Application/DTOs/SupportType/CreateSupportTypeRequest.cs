namespace HelpDesk.Application.DTOs.SupportType;

public class CreateSupportTypeRequest
{
    public int DepartmentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
