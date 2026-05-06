using HelpDesk.Domain.Entities;
using HelpDesk.Domain.Interfaces;
using HelpDesk.Infrastructure.Persistence.Contexts;

namespace HelpDesk.Infrastructure.Repositories;

public class TicketAttachmentRepository : Repository<TicketAttachment>, ITicketAttachmentRepository
{
    public TicketAttachmentRepository(HelpDeskDbContext context) : base(context) { }
}
