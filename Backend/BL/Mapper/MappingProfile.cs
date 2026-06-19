using AutoMapper;
using DAL.Entities;
using BL.DTOs;

namespace BL.Mapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User to UserResponseDto mapping
            CreateMap<User, UserResponseDto>()
                .ForMember(dest => dest.CountryName, opt => opt.MapFrom(src => src.Country != null ? src.Country.Name : null))
                .ForMember(dest => dest.GovernorateName, opt => opt.MapFrom(src => src.Governorate != null ? src.Governorate.Name : null))
                .ForMember(dest => dest.CityName, opt => opt.MapFrom(src => src.City != null ? src.City.Name : null));

            // UserRegisterDto to User mapping
            CreateMap<UserRegisterDto, User>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.EmailConfirmed, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.PhoneNumberConfirmed, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.TwoFactorEnabled, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.LockoutEnabled, opt => opt.MapFrom(src => true));

            // Location mappings
            CreateMap<Country, Country>().ReverseMap();
            CreateMap<Governorate, Governorate>().ReverseMap();
            CreateMap<City, City>().ReverseMap();
        }
    }
}
