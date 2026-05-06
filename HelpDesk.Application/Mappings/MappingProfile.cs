using AutoMapper;
using HelpDesk.Application.DTOs.Dashboard;
using HelpDesk.Application.DTOs.Department;
using HelpDesk.Application.DTOs.Score;
using HelpDesk.Application.DTOs.SlaConfiguration;
using HelpDesk.Application.DTOs.SupportType;
using HelpDesk.Application.DTOs.SupportTypeAgent;
using HelpDesk.Application.DTOs.Ticket;
using HelpDesk.Application.DTOs.TicketAttachment;
using HelpDesk.Application.DTOs.TicketComment;
using HelpDesk.Domain.Entities;

namespace HelpDesk.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Ticket
        CreateMap<Ticket, TicketDto>()
            .ForMember(dest => dest.DepartmentName,
                opt => opt.MapFrom(src => src.Department != null ? src.Department.Name : string.Empty))
            .ForMember(dest => dest.SupportTypeName,
                opt => opt.MapFrom(src => src.SupportType != null ? src.SupportType.Name : string.Empty))
            .ForMember(dest => dest.Priority,
                opt => opt.MapFrom(src => src.Priority.ToString()))
            .ForMember(dest => dest.Status,
                opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.ResolutionCategory,
                opt => opt.MapFrom(src => src.ResolutionCategory.HasValue ? src.ResolutionCategory.Value.ToString() : null))
            .ForMember(dest => dest.AssignedAgentUsername,
                opt => opt.MapFrom(src => src.AssignedUserId));

        CreateMap<Ticket, TicketSummaryDto>()
            .ForMember(dest => dest.DepartmentName,
                opt => opt.MapFrom(src => src.Department != null ? src.Department.Name : string.Empty))
            .ForMember(dest => dest.SupportTypeName,
                opt => opt.MapFrom(src => src.SupportType != null ? src.SupportType.Name : string.Empty))
            .ForMember(dest => dest.Priority,
                opt => opt.MapFrom(src => src.Priority.ToString()))
            .ForMember(dest => dest.Status,
                opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.AssignedAgentUsername,
                opt => opt.MapFrom(src => src.AssignedUserId))
            .ForMember(dest => dest.RemainingSlaPct,
                opt => opt.MapFrom(src => ComputeRemainingSlaPct(src)));

        CreateMap<CreateTicketRequest, Ticket>()
            .ForMember(dest => dest.TicketId, opt => opt.Ignore())
            .ForMember(dest => dest.TicketNumber, opt => opt.Ignore())
            .ForMember(dest => dest.Priority, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.ResolutionCategory, opt => opt.Ignore())
            .ForMember(dest => dest.AssignedUserId, opt => opt.Ignore())
            .ForMember(dest => dest.RequestedAt, opt => opt.Ignore())
            .ForMember(dest => dest.FirstOpenedAt, opt => opt.Ignore())
            .ForMember(dest => dest.WorkStartedAt, opt => opt.Ignore())
            .ForMember(dest => dest.ClosedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Deadline, opt => opt.Ignore())
            .ForMember(dest => dest.PausedAt, opt => opt.Ignore())
            .ForMember(dest => dest.TotalPausedMinutes, opt => opt.Ignore())
            .ForMember(dest => dest.RelatedTicketId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsEnabled, opt => opt.Ignore())
            .ForMember(dest => dest.Department, opt => opt.Ignore())
            .ForMember(dest => dest.SupportType, opt => opt.Ignore())
            .ForMember(dest => dest.Comments, opt => opt.Ignore())
            .ForMember(dest => dest.Attachments, opt => opt.Ignore())
            .ForMember(dest => dest.History, opt => opt.Ignore());

        // TicketComment
        CreateMap<TicketComment, TicketCommentDto>()
            .ForMember(dest => dest.Visibility,
                opt => opt.MapFrom(src => src.Visibility.ToString()));

        CreateMap<AddCommentRequest, TicketComment>()
            .ForMember(dest => dest.TicketCommentId, opt => opt.Ignore())
            .ForMember(dest => dest.TicketId, opt => opt.Ignore())
            .ForMember(dest => dest.Visibility, opt => opt.Ignore())
            .ForMember(dest => dest.AuthorId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Ticket, opt => opt.Ignore())
            .ForMember(dest => dest.Attachments, opt => opt.Ignore());

        // TicketAttachment
        CreateMap<TicketAttachment, TicketAttachmentDto>();

        // Department
        CreateMap<Department, DepartmentDto>();
        CreateMap<CreateDepartmentRequest, Department>()
            .ForMember(dest => dest.DepartmentId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.LastModifiedAt, opt => opt.Ignore())
            .ForMember(dest => dest.LastModifiedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsEnabled, opt => opt.Ignore())
            .ForMember(dest => dest.DisabledAt, opt => opt.Ignore())
            .ForMember(dest => dest.DisabledBy, opt => opt.Ignore())
            .ForMember(dest => dest.SupportTypes, opt => opt.Ignore())
            .ForMember(dest => dest.Tickets, opt => opt.Ignore());

        // SupportType
        CreateMap<SupportType, SupportTypeDto>();
        CreateMap<CreateSupportTypeRequest, SupportType>()
            .ForMember(dest => dest.SupportTypeId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.LastModifiedAt, opt => opt.Ignore())
            .ForMember(dest => dest.LastModifiedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsEnabled, opt => opt.Ignore())
            .ForMember(dest => dest.DisabledAt, opt => opt.Ignore())
            .ForMember(dest => dest.DisabledBy, opt => opt.Ignore())
            .ForMember(dest => dest.Department, opt => opt.Ignore())
            .ForMember(dest => dest.SupportTypeAgents, opt => opt.Ignore())
            .ForMember(dest => dest.Tickets, opt => opt.Ignore());

        // SupportTypeAgent
        CreateMap<SupportTypeAgent, SupportTypeAgentDto>()
            .ForMember(dest => dest.SupportTypeName,
                opt => opt.MapFrom(src => src.SupportType != null ? src.SupportType.Name : string.Empty));
        CreateMap<AssignAgentRequest, SupportTypeAgent>()
            .ForMember(dest => dest.SupportTypeAgentId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.LastModifiedAt, opt => opt.Ignore())
            .ForMember(dest => dest.LastModifiedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsEnabled, opt => opt.Ignore())
            .ForMember(dest => dest.DisabledAt, opt => opt.Ignore())
            .ForMember(dest => dest.DisabledBy, opt => opt.Ignore())
            .ForMember(dest => dest.SupportType, opt => opt.Ignore());

        // SlaConfiguration
        CreateMap<SlaConfiguration, SlaConfigurationDto>()
            .ForMember(dest => dest.Priority,
                opt => opt.MapFrom(src => src.Priority.ToString()));

        CreateMap<UpdateSlaConfigurationRequest, SlaConfiguration>()
            .ForMember(dest => dest.SlaConfigurationId, opt => opt.Ignore())
            .ForMember(dest => dest.Priority, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.LastModifiedAt, opt => opt.Ignore())
            .ForMember(dest => dest.LastModifiedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsEnabled, opt => opt.Ignore())
            .ForMember(dest => dest.DisabledAt, opt => opt.Ignore())
            .ForMember(dest => dest.DisabledBy, opt => opt.Ignore());

        // Score
        CreateMap<UserScore, UserScoreDto>()
            .ForMember(dest => dest.Level,
                opt => opt.MapFrom(src => src.Level.ToString()));

        CreateMap<ScoreTransaction, ScoreTransactionDto>()
            .ForMember(dest => dest.Reason,
                opt => opt.MapFrom(src => src.Reason.ToString()));
    }

    private static double ComputeRemainingSlaPct(Ticket ticket)
    {
        var total = (ticket.Deadline - ticket.RequestedAt).TotalSeconds;
        if (total <= 0) return 0;
        var remaining = (ticket.Deadline - DateTime.UtcNow).TotalSeconds;
        return Math.Round(Math.Clamp(remaining / total * 100, 0, 100), 1);
    }
}
