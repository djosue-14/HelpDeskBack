using HelpDesk.Application.Common;
using HelpDesk.Application.DTOs.Department;

namespace HelpDesk.Application.Interfaces;

public interface IDepartmentService
{
    Task<Result<IEnumerable<DepartmentDto>>> GetAllAsync(CancellationToken cancellationToken);
    Task<Result<DepartmentDto>> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<Result<DepartmentDto>> CreateAsync(CreateDepartmentRequest request, string createdBy, CancellationToken cancellationToken);
    Task<Result> DeleteAsync(int id, string deletedBy, CancellationToken cancellationToken);
}
