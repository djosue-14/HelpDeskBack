using AutoMapper;
using FluentValidation;
using HelpDesk.Application.Common;
using HelpDesk.Application.DTOs.TicketComment;
using HelpDesk.Application.Interfaces;
using HelpDesk.Domain.Entities;
using HelpDesk.Domain.Enums;
using HelpDesk.Domain.Interfaces;

namespace HelpDesk.Application.Services;

public class TicketCommentService : ITicketCommentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<AddCommentRequest> _addCommentValidator;

    public TicketCommentService(IUnitOfWork unitOfWork, IMapper mapper, IValidator<AddCommentRequest> addCommentValidator)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _addCommentValidator = addCommentValidator;
    }

    public async Task<Result<IEnumerable<TicketCommentDto>>> GetByTicketIdAsync(int ticketId, CancellationToken cancellationToken)
    {
        var ticket = await _unitOfWork.Tickets.GetByIdAsync(ticketId, cancellationToken);
        if (ticket is null || !ticket.IsEnabled)
            return Result<IEnumerable<TicketCommentDto>>.Failure($"Ticket {ticketId} not found");

        var comments = await _unitOfWork.TicketComments
            .FindAsync(c => c.TicketId == ticketId, cancellationToken);

        return Result<IEnumerable<TicketCommentDto>>.Success(_mapper.Map<IEnumerable<TicketCommentDto>>(comments));
    }

    public async Task<Result<TicketCommentDto>> AddCommentAsync(int ticketId, AddCommentRequest request, string authorId, CancellationToken cancellationToken)
    {
        var validation = await _addCommentValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Result<TicketCommentDto>.Failure(string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)));

        var ticket = await _unitOfWork.Tickets.GetByIdAsync(ticketId, cancellationToken);
        if (ticket is null || !ticket.IsEnabled)
            return Result<TicketCommentDto>.Failure($"Ticket {ticketId} not found");

        if (!Enum.TryParse<CommentVisibility>(request.Visibility, ignoreCase: true, out var visibility))
            return Result<TicketCommentDto>.Failure($"Invalid visibility: {request.Visibility}. Must be 'Public' or 'Internal'");

        var comment = new TicketComment
        {
            TicketId = ticketId,
            Content = request.Content,
            AuthorId = authorId,
            Visibility = visibility,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.TicketComments.AddAsync(comment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<TicketCommentDto>.Success(_mapper.Map<TicketCommentDto>(comment));
    }
}
