using HelpDesk.Application.Common;
using HelpDesk.Application.DTOs.SupportTypeAgent;

namespace HelpDesk.Application.Interfaces;

public interface ISupportTypeAgentService
{
    Task<Result<SupportTypeAgentDto>> GetActiveAgentAsync(int supportTypeId, CancellationToken cancellationToken);
    Task<Result<IEnumerable<SupportTypeAgentDto>>> GetHistoryAsync(int supportTypeId, CancellationToken cancellationToken);
    Task<Result<SupportTypeAgentDto>> AssignAsync(AssignAgentRequest request, string assignedBy, CancellationToken cancellationToken);
    Task<Result> UnassignAsync(int supportTypeId, string unassignedBy, CancellationToken cancellationToken);
}
