using HelpDesk.Domain.Entities;

namespace HelpDesk.Domain.Interfaces;

public interface ISupportTypeRepository : IRepository<SupportType>
{
    Task<IEnumerable<SupportType>> GetByDepartmentAsync(int departmentId, CancellationToken cancellationToken = default);
}
