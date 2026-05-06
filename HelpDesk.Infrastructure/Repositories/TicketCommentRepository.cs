using HelpDesk.Domain.Entities;
using HelpDesk.Domain.Interfaces;
using HelpDesk.Infrastructure.Persistence.Contexts;

namespace HelpDesk.Infrastructure.Repositories;

public class TicketCommentRepository : Repository<TicketComment>, ITicketCommentRepository
{
    public TicketCommentRepository(HelpDeskDbContext context) : base(context) { }
}
