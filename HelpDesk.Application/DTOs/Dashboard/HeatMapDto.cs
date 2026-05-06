namespace HelpDesk.Application.DTOs.Dashboard;

public class HeatMapDto
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public IEnumerable<HeatMapRowDto> Rows { get; set; } = Enumerable.Empty<HeatMapRowDto>();
}

public class HeatMapRowDto
{
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public IEnumerable<HeatMapCellDto> Cells { get; set; } = Enumerable.Empty<HeatMapCellDto>();
}

public class HeatMapCellDto
{
    public int SupportTypeId { get; set; }
    public string SupportTypeName { get; set; } = string.Empty;
    public int Volume { get; set; }
    public double SlaBreachPct { get; set; }
}
