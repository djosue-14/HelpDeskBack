using AutoMapper;
using FluentValidation;
using HelpDesk.Application.Common;
using HelpDesk.Application.DTOs.Department;
using HelpDesk.Application.Interfaces;
using HelpDesk.Domain.Entities;
using HelpDesk.Domain.Interfaces;

namespace HelpDesk.Application.Services;

public class DepartmentService : IDepartmentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateDepartmentRequest> _createValidator;

    public DepartmentService(IUnitOfWork unitOfWork, IMapper mapper, IValidator<CreateDepartmentRequest> createValidator)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _createValidator = createValidator;
    }

    public async Task<Result<IEnumerable<DepartmentDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var departments = await _unitOfWork.Departments.FindAsync(d => d.IsEnabled, cancellationToken);
        return Result<IEnumerable<DepartmentDto>>.Success(_mapper.Map<IEnumerable<DepartmentDto>>(departments));
    }

    public async Task<Result<DepartmentDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var department = await _unitOfWork.Departments.GetByIdAsync(id, cancellationToken);
        if (department is null || !department.IsEnabled)
            return Result<DepartmentDto>.Failure($"Department {id} not found");

        return Result<DepartmentDto>.Success(_mapper.Map<DepartmentDto>(department));
    }

    public async Task<Result<DepartmentDto>> CreateAsync(CreateDepartmentRequest request, string createdBy, CancellationToken cancellationToken = default)
    {
        var validation = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Result<DepartmentDto>.Failure(string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)));

        var department = _mapper.Map<Department>(request);
        department.CreatedAt = DateTime.UtcNow;
        department.CreatedBy = createdBy;
        department.IsEnabled = true;

        await _unitOfWork.Departments.AddAsync(department, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<DepartmentDto>.Success(_mapper.Map<DepartmentDto>(department));
    }

    public async Task<Result> DeleteAsync(int id, string deletedBy, CancellationToken cancellationToken = default)
    {
        var department = await _unitOfWork.Departments.GetByIdAsync(id, cancellationToken);
        if (department is null || !department.IsEnabled)
            return Result.Failure($"Department {id} not found");

        var hasActiveTickets = await _unitOfWork.Tickets
            .AnyAsync(t => t.DepartmentId == id && t.IsEnabled, cancellationToken);
        if (hasActiveTickets)
            return Result.Failure("Cannot delete department with active tickets");

        department.IsEnabled = false;
        department.DisabledAt = DateTime.UtcNow;
        department.DisabledBy = deletedBy;
        _unitOfWork.Departments.Update(department);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
