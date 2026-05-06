using AutoMapper;
using FluentValidation;
using HelpDesk.Application.Common;
using HelpDesk.Application.DTOs.SlaConfiguration;
using HelpDesk.Application.Interfaces;
using HelpDesk.Domain.Enums;
using HelpDesk.Domain.Interfaces;

namespace HelpDesk.Application.Services;

public class SlaConfigurationService : ISlaConfigurationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<UpdateSlaConfigurationRequest> _updateValidator;

    public SlaConfigurationService(IUnitOfWork unitOfWork, IMapper mapper, IValidator<UpdateSlaConfigurationRequest> updateValidator)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _updateValidator = updateValidator;
    }

    public async Task<Result<IEnumerable<SlaConfigurationDto>>> GetAllAsync(CancellationToken cancellationToken)
    {
        var configs = await _unitOfWork.SlaConfigurations.FindAsync(s => s.IsEnabled, cancellationToken);
        return Result<IEnumerable<SlaConfigurationDto>>.Success(_mapper.Map<IEnumerable<SlaConfigurationDto>>(configs));
    }

    public async Task<Result<SlaConfigurationDto>> UpdateAsync(string priority, UpdateSlaConfigurationRequest request, string updatedBy, CancellationToken cancellationToken)
    {
        var validation = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Result<SlaConfigurationDto>.Failure(string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)));

        if (!Enum.TryParse<TicketPriority>(priority, ignoreCase: true, out var parsedPriority))
            return Result<SlaConfigurationDto>.Failure($"Invalid priority: {priority}");

        var config = await _unitOfWork.SlaConfigurations
            .FirstOrDefaultAsync(s => s.Priority == parsedPriority && s.IsEnabled, cancellationToken);

        if (config is null)
            return Result<SlaConfigurationDto>.Failure($"SlaConfiguration for priority {priority} not found");

        config.HoursLimit = request.HoursLimit;
        config.LastModifiedAt = DateTime.UtcNow;
        config.LastModifiedBy = updatedBy;
        _unitOfWork.SlaConfigurations.Update(config);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<SlaConfigurationDto>.Success(_mapper.Map<SlaConfigurationDto>(config));
    }
}
