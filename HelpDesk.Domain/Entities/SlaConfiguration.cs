using HelpDesk.Domain.Common;
using HelpDesk.Domain.Enums;

namespace HelpDesk.Domain.Entities;

public class SlaConfiguration : BaseAuditableEntity
{
    public int SlaConfigurationId { get; set; }
    public TicketPriority Priority { get; set; }
    public int HoursLimit { get; set; }
}
