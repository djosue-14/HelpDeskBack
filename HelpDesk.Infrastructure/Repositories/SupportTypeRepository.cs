using HelpDesk.Domain.Entities;
using HelpDesk.Domain.Interfaces;
using HelpDesk.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Infrastructure.Repositories;

public class SupportTypeRepository : Repository<SupportType>, ISupportTypeRepository
{
    public SupportTypeRepository(HelpDeskDbContext context) : base(context) { }

    public async Task<IEnumerable<SupportType>> GetByDepartmentAsync(int departmentId, CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(st => st.DepartmentId == departmentId && st.IsEnabled)
            .ToListAsync(cancellationToken);
}
