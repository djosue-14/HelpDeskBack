using HelpDesk.Domain.Entities;
using HelpDesk.Domain.Interfaces;
using HelpDesk.Infrastructure.Persistence.Contexts;

namespace HelpDesk.Infrastructure.Repositories;

public class ScoreTransactionRepository : Repository<ScoreTransaction>, IScoreTransactionRepository
{
    public ScoreTransactionRepository(HelpDeskDbContext context) : base(context) { }
}
