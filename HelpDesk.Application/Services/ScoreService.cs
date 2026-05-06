using AutoMapper;
using HelpDesk.Application.Common;
using HelpDesk.Application.DTOs.Score;
using HelpDesk.Application.Interfaces;
using HelpDesk.Domain.Entities;
using HelpDesk.Domain.Enums;
using HelpDesk.Domain.Interfaces;

namespace HelpDesk.Application.Services;

public class ScoreService : IScoreService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    // Points per transaction reason
    private static readonly Dictionary<ScoreTransactionReason, int> PointsMap = new()
    {
        [ScoreTransactionReason.RatingSubmitted]   = 10,
        [ScoreTransactionReason.RatingWithComment] = 5,
        [ScoreTransactionReason.PenaltyRejected]   = -5,
        [ScoreTransactionReason.PenaltyDuplicate]  = -5,
        [ScoreTransactionReason.PenaltyRedirected] = -3,
    };

    public ScoreService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<UserScoreDto>> GetByUserIdAsync(string userId, CancellationToken cancellationToken)
    {
        var score = await _unitOfWork.UserScores
            .FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);

        if (score is null)
            return Result<UserScoreDto>.Failure($"Score profile not found for user {userId}");

        return Result<UserScoreDto>.Success(_mapper.Map<UserScoreDto>(score));
    }

    public async Task<Result> ApplyRatingPointsAsync(int ticketId, string raterId, bool hasComment, CancellationToken cancellationToken)
    {
        var ticket = await _unitOfWork.Tickets.GetByIdAsync(ticketId, cancellationToken);
        if (ticket is null || !ticket.IsEnabled)
            return Result.Failure($"Ticket {ticketId} not found");

        if (ticket.Status != TicketStatus.Closed)
            return Result.Failure("Ticket must be closed to be rated");

        if (ticket.CreatedBy != raterId)
            return Result.Failure("Only the ticket requester can rate it");

        var alreadyRated = await _unitOfWork.ScoreTransactions
            .AnyAsync(t => t.TicketId == ticketId && t.Reason == ScoreTransactionReason.RatingSubmitted, cancellationToken);
        if (alreadyRated)
            return Result.Failure("Ticket has already been rated");

        var agentId = ticket.AssignedUserId;
        if (string.IsNullOrEmpty(agentId))
            return Result.Failure("Ticket has no assigned agent");

        var userScore = await GetOrCreateUserScore(agentId, cancellationToken);

        await AddTransaction(userScore, ticketId, ScoreTransactionReason.RatingSubmitted, cancellationToken);

        if (hasComment)
            await AddTransaction(userScore, ticketId, ScoreTransactionReason.RatingWithComment, cancellationToken);

        RecalculateLevel(userScore);
        _unitOfWork.UserScores.Update(userScore);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> ApplyPenaltyAsync(int ticketId, string agentId, ScoreTransactionReason reason, CancellationToken cancellationToken)
    {
        if (reason is not (ScoreTransactionReason.PenaltyRejected
            or ScoreTransactionReason.PenaltyDuplicate
            or ScoreTransactionReason.PenaltyRedirected))
            return Result.Failure($"Reason {reason} is not a valid penalty");

        var ticket = await _unitOfWork.Tickets.GetByIdAsync(ticketId, cancellationToken);
        if (ticket is null || !ticket.IsEnabled)
            return Result.Failure($"Ticket {ticketId} not found");

        var userScore = await GetOrCreateUserScore(agentId, cancellationToken);

        await AddTransaction(userScore, ticketId, reason, cancellationToken);
        RecalculateLevel(userScore);
        _unitOfWork.UserScores.Update(userScore);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private async Task<UserScore> GetOrCreateUserScore(string userId, CancellationToken cancellationToken)
    {
        var score = await _unitOfWork.UserScores
            .FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);

        if (score is not null)
            return score;

        score = new UserScore
        {
            UserId = userId,
            CurrentPoints = 0,
            Level = ScoreLevel.Bronze
        };
        await _unitOfWork.UserScores.AddAsync(score, cancellationToken);
        return score;
    }

    private async Task AddTransaction(UserScore userScore, int ticketId, ScoreTransactionReason reason, CancellationToken cancellationToken)
    {
        var points = PointsMap.GetValueOrDefault(reason, 0);
        userScore.CurrentPoints = Math.Max(0, userScore.CurrentPoints + points);

        var transaction = new ScoreTransaction
        {
            UserId = userScore.UserId,
            TicketId = ticketId,
            Points = points,
            Reason = reason,
            CreatedAt = DateTime.UtcNow
        };
        await _unitOfWork.ScoreTransactions.AddAsync(transaction, cancellationToken);
    }

    private static void RecalculateLevel(UserScore userScore)
    {
        userScore.Level = userScore.CurrentPoints switch
        {
            >= 300 => ScoreLevel.Gold,
            >= 100 => ScoreLevel.Silver,
            _      => ScoreLevel.Bronze
        };
    }
}
