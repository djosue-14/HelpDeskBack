namespace HelpDesk.Application.DTOs.Score;

public class UserScoreDto
{
    public int UserScoreId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int CurrentPoints { get; set; }
    public string Level { get; set; } = string.Empty;
    public IEnumerable<ScoreTransactionDto> ScoreTransactions { get; set; } = Enumerable.Empty<ScoreTransactionDto>();
}
