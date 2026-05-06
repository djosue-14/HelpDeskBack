namespace HelpDesk.Application.DTOs.Dashboard;

public class AdminDashboardDto
{
    public int TotalActiveAllDepartments { get; set; }
    public double SlaDailyCompliancePct { get; set; }
    public Dictionary<string, int> CountByPriority { get; set; } = new();
    public Dictionary<string, int> CountByDepartment { get; set; } = new();
}
