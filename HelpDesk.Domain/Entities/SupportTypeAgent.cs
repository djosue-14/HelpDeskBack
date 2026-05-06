using HelpDesk.Domain.Common;

namespace HelpDesk.Domain.Entities;

public class SupportTypeAgent : BaseAuditableEntity
{
    public int SupportTypeAgentId { get; set; }
    public int SupportTypeId { get; set; }
    public SupportType SupportType { get; set; } = null!;
    public string UserId { get; set; } = string.Empty;
}
