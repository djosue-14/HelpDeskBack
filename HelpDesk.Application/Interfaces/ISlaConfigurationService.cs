using HelpDesk.Application.Common;
using HelpDesk.Application.DTOs.SlaConfiguration;

namespace HelpDesk.Application.Interfaces;

public interface ISlaConfigurationService
{
    Task<Result<IEnumerable<SlaConfigurationDto>>> GetAllAsync(CancellationToken cancellationToken);
    Task<Result<SlaConfigurationDto>> UpdateAsync(string priority, UpdateSlaConfigurationRequest request, string updatedBy, CancellationToken cancellationToken);
}
