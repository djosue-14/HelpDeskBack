using HelpDesk.Domain.Entities;
using HelpDesk.Domain.Interfaces;
using HelpDesk.Infrastructure.Persistence.Contexts;

namespace HelpDesk.Infrastructure.Repositories;

public class SupportTypeAgentRepository : Repository<SupportTypeAgent>, ISupportTypeAgentRepository
{
    public SupportTypeAgentRepository(HelpDeskDbContext context) : base(context) { }
}
