using AutoMapper;
using DAL.Entities;
using BL.DTOs;
using BL.DTOs.Education;
using BL.DTOs.Experience;
using BL.DTOs.Certificate;
using BL.DTOs.Project;
using BL.DTOs.UserTopicMastery;
using BL.DTOs.ExamSession;

namespace BL.Mapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User to UserResponseDto mapping (location + social links as strings)
            CreateMap<User, UserResponseDto>()
                .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Country))
                .ForMember(dest => dest.State, opt => opt.MapFrom(src => src.State))
                .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City))
                .ForMember(dest => dest.LinkedIn, opt => opt.MapFrom(src => src.LinkedIn))
                .ForMember(dest => dest.GitHub, opt => opt.MapFrom(src => src.GitHub))
                //.ForMember(dest => dest.Roles, opt => opt.Ignore()) // Roles will be set manually in service
                .ForMember(dest => dest.Certificates, opt => opt.MapFrom(src => src.Certificates))
                .ForMember(dest => dest.Educations, opt => opt.MapFrom(src => src.Educations))
                .ForMember(dest => dest.Experiences, opt => opt.MapFrom(src => src.Experiences))
                .ForMember(dest => dest.Projects, opt => opt.MapFrom(src => src.Projects))
                .ForMember(dest => dest.UserTopicMasteries, opt => opt.MapFrom(src => src.TopicMasteries));

            // Map child collections
            CreateMap<Certificate, CertificateDto>().ReverseMap();
            CreateMap<Education, EducationDto>().ReverseMap();
            CreateMap<Experience, ExperienceDto>().ReverseMap();
            CreateMap<Project, ProjectDto>().ReverseMap();
            CreateMap<UserTopicMastery, UserTopicMasteryDto>()
            .ForMember( dest => dest.TopicName, opt => opt.MapFrom(src => src.Topic.Name));

            // ExamSession mappings
            CreateMap<ExamSession, ExamSessionDto>()
                .ForMember(dest => dest.StudySession, opt => opt.MapFrom(src => src.StudySession));
            CreateMap<StudySession, StudySessionDto>();
            CreateMap<SessionAnswer, SessionAnswerDto>().ReverseMap();
            CreateMap<AnswerChoice, AnswerChoiceDto>().ReverseMap();


            CreateMap<AddExamSessionDto, ExamSession>();
            CreateMap<AddSessionAnswerDto, SessionAnswer>();
            CreateMap<AddAnswerChoiceDto, AnswerChoice>();

            CreateMap<Resource, ResourceBriefDto>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.Url));

            // UserRegisterDto to User mapping
            CreateMap<UserRegisterDto, User>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.EmailConfirmed, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.PhoneNumberConfirmed, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.TwoFactorEnabled, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.LockoutEnabled, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.Goals, opt => opt.Ignore()) // Goals are handled separately in RegisterUserAsync
                .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Country))
                .ForMember(dest => dest.State, opt => opt.MapFrom(src => src.State))
                .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City))
                .ForMember(dest => dest.LinkedIn, opt => opt.MapFrom(src => src.LinkedIn))
                .ForMember(dest => dest.GitHub, opt => opt.MapFrom(src => src.GitHub));

            // SubscriptionPlan mapping
            CreateMap<SubscriptionPlan, SubscriptionPlanDto>()
                .ForMember(dest => dest.PlanType, opt => opt.MapFrom(src => src.PlanType.ToString()));

            // Subscription mapping
            CreateMap<Subscription, SubscriptionDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            //goal mapping
            CreateMap<Goal, GoalDTO>().ReverseMap();
        }
    }
}
