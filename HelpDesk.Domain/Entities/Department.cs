using HelpDesk.Domain.Common;

namespace HelpDesk.Domain.Entities;

public class Department : BaseAuditableEntity
{
    public int DepartmentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? CoordinatorUserId { get; set; }
    public ICollection<SupportType> SupportTypes { get; set; } = new List<SupportType>();
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
