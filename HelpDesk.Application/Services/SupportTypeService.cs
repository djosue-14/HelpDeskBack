using AutoMapper;
using FluentValidation;
using HelpDesk.Application.Common;
using HelpDesk.Application.DTOs.SupportType;
using HelpDesk.Application.Interfaces;
using HelpDesk.Domain.Entities;
using HelpDesk.Domain.Interfaces;

namespace HelpDesk.Application.Services;

public class SupportTypeService : ISupportTypeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateSupportTypeRequest> _createValidator;

    public SupportTypeService(IUnitOfWork unitOfWork, IMapper mapper, IValidator<CreateSupportTypeRequest> createValidator)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _createValidator = createValidator;
    }

    public async Task<Result<IEnumerable<SupportTypeDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var types = await _unitOfWork.SupportTypes.FindAsync(s => s.IsEnabled, cancellationToken);
        return Result<IEnumerable<SupportTypeDto>>.Success(_mapper.Map<IEnumerable<SupportTypeDto>>(types));
    }

    public async Task<Result<SupportTypeDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var supportType = await _unitOfWork.SupportTypes.GetByIdAsync(id, cancellationToken);
        if (supportType is null || !supportType.IsEnabled)
            return Result<SupportTypeDto>.Failure($"SupportType {id} not found");

        return Result<SupportTypeDto>.Success(_mapper.Map<SupportTypeDto>(supportType));
    }

    public async Task<Result<IEnumerable<SupportTypeDto>>> GetByDepartmentAsync(int departmentId, CancellationToken cancellationToken = default)
    {
        var department = await _unitOfWork.Departments.GetByIdAsync(departmentId, cancellationToken);
        if (department is null || !department.IsEnabled)
            return Result<IEnumerable<SupportTypeDto>>.Failure($"Department {departmentId} not found");

        var types = await _unitOfWork.SupportTypes.GetByDepartmentAsync(departmentId, cancellationToken);
        return Result<IEnumerable<SupportTypeDto>>.Success(_mapper.Map<IEnumerable<SupportTypeDto>>(types));
    }

    public async Task<Result<SupportTypeDto>> CreateAsync(CreateSupportTypeRequest request, string createdBy, CancellationToken cancellationToken = default)
    {
        var validation = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Result<SupportTypeDto>.Failure(string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)));

        var department = await _unitOfWork.Departments.GetByIdAsync(request.DepartmentId, cancellationToken);
        if (department is null || !department.IsEnabled)
            return Result<SupportTypeDto>.Failure($"Department {request.DepartmentId} not found");

        var supportType = _mapper.Map<SupportType>(request);
        supportType.CreatedAt = DateTime.UtcNow;
        supportType.CreatedBy = createdBy;
        supportType.IsEnabled = true;

        await _unitOfWork.SupportTypes.AddAsync(supportType, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<SupportTypeDto>.Success(_mapper.Map<SupportTypeDto>(supportType));
    }

    public async Task<Result> DeleteAsync(int id, string deletedBy, CancellationToken cancellationToken = default)
    {
        var supportType = await _unitOfWork.SupportTypes.GetByIdAsync(id, cancellationToken);
        if (supportType is null || !supportType.IsEnabled)
            return Result.Failure($"SupportType {id} not found");

        var hasActiveTickets = await _unitOfWork.Tickets
            .AnyAsync(t => t.SupportTypeId == id && t.IsEnabled, cancellationToken);
        if (hasActiveTickets)
            return Result.Failure("Cannot delete support type with active tickets");

        supportType.IsEnabled = false;
        supportType.DisabledAt = DateTime.UtcNow;
        supportType.DisabledBy = deletedBy;
        _unitOfWork.SupportTypes.Update(supportType);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
