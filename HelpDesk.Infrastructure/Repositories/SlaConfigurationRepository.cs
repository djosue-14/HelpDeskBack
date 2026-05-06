using HelpDesk.Domain.Entities;
using HelpDesk.Domain.Interfaces;
using HelpDesk.Infrastructure.Persistence.Contexts;

namespace HelpDesk.Infrastructure.Repositories;

public class SlaConfigurationRepository : Repository<SlaConfiguration>, ISlaConfigurationRepository
{
    public SlaConfigurationRepository(HelpDeskDbContext context) : base(context) { }
}
