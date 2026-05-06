using HelpDesk.Domain.Common;

namespace HelpDesk.Domain.Entities;

public class SupportType : BaseAuditableEntity
{
    public int SupportTypeId { get; set; }
    public int DepartmentId { get; set; }
    public Department Department { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ICollection<SupportTypeAgent> SupportTypeAgents { get; set; } = new List<SupportTypeAgent>();
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
