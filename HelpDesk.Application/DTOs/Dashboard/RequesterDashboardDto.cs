using HelpDesk.Application.DTOs.Score;
using HelpDesk.Application.DTOs.Ticket;

namespace HelpDesk.Application.DTOs.Dashboard;

public class RequesterDashboardDto
{
    public IEnumerable<TicketSummaryDto> ActiveTickets { get; set; } = Enumerable.Empty<TicketSummaryDto>();
    public Dictionary<string, int> TicketCountByStatus { get; set; } = new();
    public UserScoreDto Score { get; set; } = new();
}
