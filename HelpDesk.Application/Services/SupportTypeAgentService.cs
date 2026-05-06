using AutoMapper;
using FluentValidation;
using HelpDesk.Application.Common;
using HelpDesk.Application.DTOs.SupportTypeAgent;
using HelpDesk.Application.Interfaces;
using HelpDesk.Domain.Entities;
using HelpDesk.Domain.Interfaces;

namespace HelpDesk.Application.Services;

public class SupportTypeAgentService : ISupportTypeAgentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<AssignAgentRequest> _assignValidator;

    public SupportTypeAgentService(IUnitOfWork unitOfWork, IMapper mapper, IValidator<AssignAgentRequest> assignValidator)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _assignValidator = assignValidator;
    }

    public async Task<Result<SupportTypeAgentDto>> GetActiveAgentAsync(int supportTypeId, CancellationToken cancellationToken)
    {
        var assignment = await _unitOfWork.SupportTypeAgents
            .FirstOrDefaultAsync(a => a.SupportTypeId == supportTypeId && a.IsEnabled, cancellationToken);

        if (assignment is null)
            return Result<SupportTypeAgentDto>.Failure($"No active agent for SupportType {supportTypeId}");

        return Result<SupportTypeAgentDto>.Success(_mapper.Map<SupportTypeAgentDto>(assignment));
    }

    public async Task<Result<IEnumerable<SupportTypeAgentDto>>> GetHistoryAsync(int supportTypeId, CancellationToken cancellationToken)
    {
        var history = await _unitOfWork.SupportTypeAgents
            .FindAsync(a => a.SupportTypeId == supportTypeId, cancellationToken);

        return Result<IEnumerable<SupportTypeAgentDto>>.Success(_mapper.Map<IEnumerable<SupportTypeAgentDto>>(history));
    }

    public async Task<Result<SupportTypeAgentDto>> AssignAsync(AssignAgentRequest request, string assignedBy, CancellationToken cancellationToken)
    {
        var validation = await _assignValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Result<SupportTypeAgentDto>.Failure(string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)));

        var supportType = await _unitOfWork.SupportTypes.GetByIdAsync(request.SupportTypeId, cancellationToken);
        if (supportType is null)
            return Result<SupportTypeAgentDto>.Failure($"SupportType {request.SupportTypeId} not found");

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var currentActive = await _unitOfWork.SupportTypeAgents
                .FirstOrDefaultAsync(a => a.SupportTypeId == request.SupportTypeId && a.IsEnabled, cancellationToken);

            if (currentActive is not null)
            {
                currentActive.IsEnabled = false;
                currentActive.DisabledAt = DateTime.UtcNow;
                currentActive.DisabledBy = assignedBy;
                _unitOfWork.SupportTypeAgents.Update(currentActive);
            }

            var newAssignment = new SupportTypeAgent
            {
                SupportTypeId = request.SupportTypeId,
                UserId = request.UserId,
                IsEnabled = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = assignedBy
            };

            await _unitOfWork.SupportTypeAgents.AddAsync(newAssignment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync();

            return Result<SupportTypeAgentDto>.Success(_mapper.Map<SupportTypeAgentDto>(newAssignment));
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<Result> UnassignAsync(int supportTypeId, string unassignedBy, CancellationToken cancellationToken)
    {
        var assignment = await _unitOfWork.SupportTypeAgents
            .FirstOrDefaultAsync(a => a.SupportTypeId == supportTypeId && a.IsEnabled, cancellationToken);

        if (assignment is null)
            return Result.Failure($"No active agent for SupportType {supportTypeId}");

        assignment.IsEnabled = false;
        assignment.DisabledAt = DateTime.UtcNow;
        assignment.DisabledBy = unassignedBy;
        _unitOfWork.SupportTypeAgents.Update(assignment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
