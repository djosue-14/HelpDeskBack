using HelpDesk.Application.Common;
using HelpDesk.Application.DTOs.SupportType;

namespace HelpDesk.Application.Interfaces;

public interface ISupportTypeService
{
    Task<Result<IEnumerable<SupportTypeDto>>> GetAllAsync(CancellationToken cancellationToken);
    Task<Result<SupportTypeDto>> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<Result<IEnumerable<SupportTypeDto>>> GetByDepartmentAsync(int departmentId, CancellationToken cancellationToken);
    Task<Result<SupportTypeDto>> CreateAsync(CreateSupportTypeRequest request, string createdBy, CancellationToken cancellationToken);
    Task<Result> DeleteAsync(int id, string deletedBy, CancellationToken cancellationToken);
}
