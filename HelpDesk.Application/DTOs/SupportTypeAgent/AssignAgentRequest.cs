namespace HelpDesk.Application.DTOs.SupportTypeAgent;

public class AssignAgentRequest
{
    public int SupportTypeId { get; set; }
    public string UserId { get; set; } = string.Empty;
}
