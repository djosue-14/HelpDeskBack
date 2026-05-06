namespace HelpDesk.Application.DTOs.Dashboard;

public class LeaderboardDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public IEnumerable<LeaderboardEntryDto> Top10 { get; set; } = Enumerable.Empty<LeaderboardEntryDto>();
}

public class LeaderboardEntryDto
{
    public int Rank { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int PointsEarned { get; set; }
    public double RatingRatePct { get; set; }
}
