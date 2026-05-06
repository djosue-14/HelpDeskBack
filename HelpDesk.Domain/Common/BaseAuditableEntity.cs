namespace HelpDesk.Domain.Common;

public abstract class BaseAuditableEntity
{
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
    public bool IsEnabled { get; set; } = true;
    public DateTime? DisabledAt { get; set; }
    public string? DisabledBy { get; set; }
}
