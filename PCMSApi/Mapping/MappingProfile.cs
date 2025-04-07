using AutoMapper;
using PCMSApi.Models;

namespace PCMSApi.Mapping
{
    /// <summary>
    /// AutoMapper profile for mapping between domain models and DTOs.
    /// </summary>
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Mapping configuration for Attachment to AttachmentDto
            CreateMap<Attachment, AttachmentDto>()
                .ForMember(dest => dest.Url, opt => opt.Ignore()); // URL is set manually

            // Mapping configuration for Patient to PatientDto
            CreateMap<Patient, PatientDto>()
                .ForMember(dest => dest.PatientId, opt => opt.MapFrom(src => src.PatientUid))
                .ForMember(dest => dest.Attachments, opt => opt.MapFrom(src => src.Attachments))
                .ForMember(dest => dest.MedicalHistory, opt => opt.NullSubstitute(new List<MedicalCondition>()));

            // Mapping configuration for PatientDto to Patient
            CreateMap<PatientDto, Patient>()
                .ForMember(dest => dest.PatientUid, opt => opt.MapFrom(src => src.PatientId))
                .ForMember(dest => dest.Attachments, opt => opt.Ignore()) // Child collections are handled separately
                .ForMember(dest => dest.MedicalHistory, opt => opt.NullSubstitute(new List<MedicalCondition>()));
        }
    }
}