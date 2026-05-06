using HelpDesk.Domain.Enums;

namespace HelpDesk.Domain.Entities;

public class UserScore
{
    public int UserScoreId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int CurrentPoints { get; set; }
    public ScoreLevel Level { get; set; }
    public ICollection<ScoreTransaction> ScoreTransactions { get; set; } = new List<ScoreTransaction>();
}
