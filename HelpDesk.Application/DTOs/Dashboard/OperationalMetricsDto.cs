namespace HelpDesk.Application.DTOs.Dashboard;

public class OperationalMetricsDto
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public int TotalCreated { get; set; }
    public int TotalClosed { get; set; }
    public int TotalActive { get; set; }
    public double SlaCompliancePct { get; set; }
    public double AvgResolutionHours { get; set; }
    public double AvgFirstResponseHours { get; set; }
    public double ReopenRatePct { get; set; }
    public double RedirectRatePct { get; set; }
    public IEnumerable<DailyVolumeDto> DailyTrend { get; set; } = Enumerable.Empty<DailyVolumeDto>();
    public Dictionary<string, double> SlaByPriority { get; set; } = new();
    public OperationalMetricsDto? PreviousPeriod { get; set; }
}

public class DailyVolumeDto
{
    public DateOnly Date { get; set; }
    public int Created { get; set; }
    public int Closed { get; set; }
}
