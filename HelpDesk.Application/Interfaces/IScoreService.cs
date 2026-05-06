using HelpDesk.Application.Common;
using HelpDesk.Application.DTOs.Score;
using HelpDesk.Domain.Enums;

namespace HelpDesk.Application.Interfaces;

public interface IScoreService
{
    Task<Result<UserScoreDto>> GetByUserIdAsync(string userId, CancellationToken cancellationToken);
    Task<Result> ApplyRatingPointsAsync(int ticketId, string userId, bool hasComment, CancellationToken cancellationToken);
    Task<Result> ApplyPenaltyAsync(int ticketId, string userId, ScoreTransactionReason reason, CancellationToken cancellationToken);
}
