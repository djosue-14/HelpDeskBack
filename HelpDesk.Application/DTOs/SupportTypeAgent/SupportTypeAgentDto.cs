namespace HelpDesk.Application.DTOs.SupportTypeAgent;

public class SupportTypeAgentDto
{
    public int SupportTypeAgentId { get; set; }
    public int SupportTypeId { get; set; }
    public string SupportTypeName { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? DisabledAt { get; set; }
    public string? DisabledBy { get; set; }
}
