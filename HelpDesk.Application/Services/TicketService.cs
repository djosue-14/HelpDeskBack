using AutoMapper;
using FluentValidation;
using HelpDesk.Application.Common;
using HelpDesk.Application.DTOs.Ticket;
using HelpDesk.Application.Interfaces;
using HelpDesk.Domain.Entities;
using HelpDesk.Domain.Enums;
using HelpDesk.Domain.Interfaces;

namespace HelpDesk.Application.Services;

public class TicketService : ITicketService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateTicketRequest> _createValidator;
    private readonly IValidator<UpdateTicketStatusRequest> _statusValidator;
    private readonly IValidator<CloseTicketRequest> _closeValidator;

    public TicketService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IValidator<CreateTicketRequest> createValidator,
        IValidator<UpdateTicketStatusRequest> statusValidator,
        IValidator<CloseTicketRequest> closeValidator)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _createValidator = createValidator;
        _statusValidator = statusValidator;
        _closeValidator = closeValidator;
    }

    public async Task<Result<TicketDto>> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var ticket = await _unitOfWork.Tickets.GetWithFullDetailAsync(id, cancellationToken);
        if (ticket is null || !ticket.IsEnabled)
            return Result<TicketDto>.Failure($"Ticket {id} not found");

        return Result<TicketDto>.Success(_mapper.Map<TicketDto>(ticket));
    }

    public async Task<Result<IEnumerable<TicketSummaryDto>>> GetAllAsync(CancellationToken cancellationToken)
    {
        var tickets = await _unitOfWork.Tickets.FindAsync(t => t.IsEnabled, cancellationToken);
        return Result<IEnumerable<TicketSummaryDto>>.Success(_mapper.Map<IEnumerable<TicketSummaryDto>>(tickets));
    }

    public async Task<Result<IEnumerable<TicketSummaryDto>>> GetByAgentAsync(string agentUserId, CancellationToken cancellationToken)
    {
        var tickets = await _unitOfWork.Tickets.GetByAgentAsync(agentUserId, cancellationToken: cancellationToken);
        return Result<IEnumerable<TicketSummaryDto>>.Success(_mapper.Map<IEnumerable<TicketSummaryDto>>(tickets));
    }

    public async Task<Result<TicketDto>> CreateAsync(CreateTicketRequest request, string createdBy, CancellationToken cancellationToken)
    {
        var validation = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Result<TicketDto>.Failure(string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)));

        var supportType = await _unitOfWork.SupportTypes.GetByIdAsync(request.SupportTypeId, cancellationToken);
        if (supportType is null)
            return Result<TicketDto>.Failure($"SupportType {request.SupportTypeId} not found");

        var department = await _unitOfWork.Departments.GetByIdAsync(request.DepartmentId, cancellationToken);
        if (department is null)
            return Result<TicketDto>.Failure($"Department {request.DepartmentId} not found");

        var priority = Enum.TryParse<TicketPriority>(request.Priority, out var parsedPriority)
            ? parsedPriority
            : TicketPriority.Medium;
        var slaConfig = await _unitOfWork.SlaConfigurations
            .FirstOrDefaultAsync(s => s.Priority == priority && s.IsEnabled, cancellationToken);

        var now = DateTime.UtcNow;
        var hoursLimit = slaConfig?.HoursLimit ?? 24;
        var deadline = now.AddHours(hoursLimit);

        var activeAgent = await _unitOfWork.SupportTypeAgents
            .FirstOrDefaultAsync(a => a.SupportTypeId == request.SupportTypeId && a.IsEnabled, cancellationToken);

        var ticketCount = await _unitOfWork.Tickets.CountAsync(_ => true, cancellationToken);

        var ticket = _mapper.Map<Ticket>(request);
        ticket.TicketNumber = ticketCount + 1;
        ticket.Priority = priority;
        ticket.Status = TicketStatus.Open;
        ticket.RequestedAt = now;
        ticket.CreatedAt = now;
        ticket.CreatedBy = createdBy;
        ticket.IsEnabled = true;
        ticket.Deadline = deadline;
        ticket.AssignedUserId = activeAgent?.UserId;
        ticket.TotalPausedMinutes = 0;

        await _unitOfWork.Tickets.AddAsync(ticket, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<TicketDto>.Success(_mapper.Map<TicketDto>(ticket));
    }

    public async Task<Result<TicketDto>> ChangeStatusAsync(int id, UpdateTicketStatusRequest request, string changedBy, CancellationToken cancellationToken)
    {
        var validation = await _statusValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Result<TicketDto>.Failure(string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)));

        if (!Enum.TryParse<TicketStatus>(request.NewStatus, ignoreCase: true, out var newStatus))
            return Result<TicketDto>.Failure($"Invalid status: {request.NewStatus}");

        var ticket = await _unitOfWork.Tickets.GetWithFullDetailAsync(id, cancellationToken);
        if (ticket is null || !ticket.IsEnabled)
            return Result<TicketDto>.Failure($"Ticket {id} not found");

        var previousStatus = ticket.Status.ToString();
        var now = DateTime.UtcNow;

        switch (newStatus)
        {
            case TicketStatus.InProgress when ticket.Status == TicketStatus.Open || ticket.Status == TicketStatus.Reopened:
                ticket.WorkStartedAt ??= now;
                if (ticket.FirstOpenedAt is null)
                    ticket.FirstOpenedAt = now;
                if (ticket.PausedAt.HasValue)
                {
                    ticket.TotalPausedMinutes += (int)(now - ticket.PausedAt.Value).TotalMinutes;
                    ticket.PausedAt = null;
                }
                break;

            case TicketStatus.WaitingForInfo when ticket.Status == TicketStatus.InProgress:
                ticket.PausedAt = now;
                break;

            case TicketStatus.Open when ticket.Status == TicketStatus.WaitingForInfo:
                if (ticket.PausedAt.HasValue)
                {
                    ticket.TotalPausedMinutes += (int)(now - ticket.PausedAt.Value).TotalMinutes;
                    ticket.PausedAt = null;
                }
                break;

            default:
                return Result<TicketDto>.Failure($"Cannot transition from {ticket.Status} to {newStatus}");
        }

        ticket.Status = newStatus;
        _unitOfWork.Tickets.Update(ticket);

        await RecordHistory(ticket.TicketId, "StatusChanged", previousStatus, newStatus.ToString(), changedBy, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<TicketDto>.Success(_mapper.Map<TicketDto>(ticket));
    }

    public async Task<Result<TicketDto>> CloseAsync(int id, CloseTicketRequest request, string closedBy, CancellationToken cancellationToken)
    {
        var validation = await _closeValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Result<TicketDto>.Failure(string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)));

        if (!Enum.TryParse<ResolutionCategory>(request.ResolutionCategory, ignoreCase: true, out var category))
            return Result<TicketDto>.Failure($"Invalid resolution category: {request.ResolutionCategory}");

        var ticket = await _unitOfWork.Tickets.GetWithFullDetailAsync(id, cancellationToken);
        if (ticket is null || !ticket.IsEnabled)
            return Result<TicketDto>.Failure($"Ticket {id} not found");

        if (ticket.Status == TicketStatus.Closed)
            return Result<TicketDto>.Failure("Ticket is already closed");

        var now = DateTime.UtcNow;
        var previousStatus = ticket.Status.ToString();

        if (ticket.PausedAt.HasValue)
        {
            ticket.TotalPausedMinutes += (int)(now - ticket.PausedAt.Value).TotalMinutes;
            ticket.PausedAt = null;
        }

        ticket.Status = TicketStatus.Closed;
        ticket.ResolutionCategory = category;
        ticket.ClosedAt = now;
        _unitOfWork.Tickets.Update(ticket);

        if (!string.IsNullOrWhiteSpace(request.ClosingComment))
        {
            var comment = new TicketComment
            {
                TicketId = id,
                Content = request.ClosingComment,
                AuthorId = closedBy,
                Visibility = CommentVisibility.Public,
                CreatedAt = now
            };
            await _unitOfWork.TicketComments.AddAsync(comment, cancellationToken);
        }

        await RecordHistory(ticket.TicketId, "Closed", previousStatus, TicketStatus.Closed.ToString(), closedBy, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<TicketDto>.Success(_mapper.Map<TicketDto>(ticket));
    }

    public async Task<Result<TicketDto>> ReopenAsync(int id, string reason, string reopenedBy, CancellationToken cancellationToken)
    {
        var ticket = await _unitOfWork.Tickets.GetWithFullDetailAsync(id, cancellationToken);
        if (ticket is null || !ticket.IsEnabled)
            return Result<TicketDto>.Failure($"Ticket {id} not found");

        if (ticket.Status != TicketStatus.Closed)
            return Result<TicketDto>.Failure("Only closed tickets can be reopened");

        ticket.Status = TicketStatus.Reopened;
        ticket.ClosedAt = null;
        ticket.ResolutionCategory = null;
        _unitOfWork.Tickets.Update(ticket);

        await RecordHistory(ticket.TicketId, "Reopened", TicketStatus.Closed.ToString(), TicketStatus.Reopened.ToString(), reopenedBy, cancellationToken, reason);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<TicketDto>.Success(_mapper.Map<TicketDto>(ticket));
    }

    public async Task<Result<TicketDto>> RedirectAsync(int id, int newSupportTypeId, string redirectedBy, CancellationToken cancellationToken)
    {
        var ticket = await _unitOfWork.Tickets.GetWithFullDetailAsync(id, cancellationToken);
        if (ticket is null || !ticket.IsEnabled)
            return Result<TicketDto>.Failure($"Ticket {id} not found");

        if (ticket.Status == TicketStatus.Closed)
            return Result<TicketDto>.Failure("Cannot redirect a closed ticket");

        var supportType = await _unitOfWork.SupportTypes.GetByIdAsync(newSupportTypeId, cancellationToken);
        if (supportType is null || !supportType.IsEnabled)
            return Result<TicketDto>.Failure($"SupportType {newSupportTypeId} not found");

        var previousSupportTypeId = ticket.SupportTypeId.ToString();
        ticket.SupportTypeId = newSupportTypeId;

        var activeAgent = await _unitOfWork.SupportTypeAgents
            .FirstOrDefaultAsync(a => a.SupportTypeId == newSupportTypeId && a.IsEnabled, cancellationToken);
        ticket.AssignedUserId = activeAgent?.UserId;

        _unitOfWork.Tickets.Update(ticket);

        await RecordHistory(ticket.TicketId, "Redirected", previousSupportTypeId, newSupportTypeId.ToString(), redirectedBy, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<TicketDto>.Success(_mapper.Map<TicketDto>(ticket));
    }

    public async Task<Result> DeleteAsync(int id, string deletedBy, CancellationToken cancellationToken)
    {
        var ticket = await _unitOfWork.Tickets.GetByIdAsync(id, cancellationToken);
        if (ticket is null || !ticket.IsEnabled)
            return Result.Failure($"Ticket {id} not found");

        ticket.IsEnabled = false;
        _unitOfWork.Tickets.Update(ticket);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private async Task RecordHistory(int ticketId, string action, string? previousValue, string? newValue, string executedBy, CancellationToken cancellationToken, string? detail = null)
    {
        var entry = new TicketHistory
        {
            TicketId = ticketId,
            ActionType = action,
            PreviousValue = previousValue,
            NewValue = detail is null ? newValue : $"{newValue} — {detail}",
            ExecutedBy = executedBy,
            ExecutedAt = DateTime.UtcNow
        };
        await _unitOfWork.TicketHistories.AddAsync(entry, cancellationToken);
    }
}
