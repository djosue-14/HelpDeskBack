using HelpDesk.Domain.Entities;
using HelpDesk.Domain.Interfaces;
using HelpDesk.Infrastructure.Persistence.Contexts;

namespace HelpDesk.Infrastructure.Repositories;

public class TicketHistoryRepository : Repository<TicketHistory>, ITicketHistoryRepository
{
    public TicketHistoryRepository(HelpDeskDbContext context) : base(context) { }
}
