namespace HelpDesk.Application.DTOs.SlaConfiguration;

public class SlaConfigurationDto
{
    public int SlaConfigurationId { get; set; }
    public string Priority { get; set; } = string.Empty;
    public int HoursLimit { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
}
