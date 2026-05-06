using HelpDesk.Domain.Entities;
using HelpDesk.Domain.Interfaces;
using HelpDesk.Infrastructure.Persistence.Contexts;

namespace HelpDesk.Infrastructure.Repositories;

public class UserScoreRepository : Repository<UserScore>, IUserScoreRepository
{
    public UserScoreRepository(HelpDeskDbContext context) : base(context) { }
}
