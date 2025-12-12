using AutoMapper;
using HealthAidAPI.Models;
using HealthAidAPI.Models.Analytics;
using HealthAidAPI.Models.Emergency;
using HealthAidAPI.Models.Location;
using HealthAidAPI.Models.MedicalFacilities;
using HealthAidAPI.Models.Recommendations;

// استيراد كافة مجلدات الـ DTOs التي أنشأناها
using HealthAidAPI.DTOs.Users;
using HealthAidAPI.DTOs.Doctors;
using HealthAidAPI.DTOs.Patients;
using HealthAidAPI.DTOs.Appointments;
using HealthAidAPI.DTOs.Consultations;
using HealthAidAPI.DTOs.Prescriptions;
using HealthAidAPI.DTOs.MedicineRequests;
using HealthAidAPI.DTOs.HealthGuides;
using HealthAidAPI.DTOs.Messages;
using HealthAidAPI.DTOs.MentalSupportSessions;
using HealthAidAPI.DTOs.Notifications;
using HealthAidAPI.DTOs.Equipments;
using HealthAidAPI.DTOs.NGOs;
using HealthAidAPI.DTOs.NgoMissions;
using HealthAidAPI.DTOs.Services;
using HealthAidAPI.DTOs.Sponsorships;
using HealthAidAPI.DTOs.Transactions;
using HealthAidAPI.DTOs.PublicAlerts;
using HealthAidAPI.DTOs.Ratings;
using HealthAidAPI.DTOs.MedicalFacilities;
using HealthAidAPI.DTOs.Emergency;
using HealthAidAPI.DTOs.Locations;
using HealthAidAPI.DTOs.Recommendations;
using HealthAidAPI.DTOs.Analytics;

namespace HealthAidAPI.Helpers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // =================================================
            // 1. Users & Auth
            // =================================================
            CreateMap<User, UserDto>();
            CreateMap<CreateUserDto, User>();
            CreateMap<UpdateUserDto, User>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null)); // تحديث الحقول غير الفارغة فقط

            // =================================================
            // 2. Doctors
            // =================================================
            CreateMap<Doctor, DoctorDto>()
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User)); // تضمين بيانات المستخدم
            CreateMap<CreateDoctorDto, Doctor>();
            CreateMap<UpdateDoctorDto, Doctor>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // =================================================
            // 3. Patients
            // =================================================
            CreateMap<Patient, PatientDto>()
                .ForMember(dest => dest.PatientName, opt => opt.MapFrom(src => $"{src.User.FirstName} {src.User.LastName}"));
            CreateMap<CreatePatientDto, Patient>();
            CreateMap<UpdatePatientDto, Patient>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // =================================================
            // 4. Appointments
            // =================================================
            CreateMap<Appointment, AppointmentDto>()
                .ForMember(dest => dest.DoctorName, opt => opt.MapFrom(src => src.Doctor != null ? $"{src.Doctor.User.FirstName} {src.Doctor.User.LastName}" : "Unknown"))
                .ForMember(dest => dest.PatientName, opt => opt.MapFrom(src => src.Patient != null ? $"{src.Patient.User.FirstName} {src.Patient.User.LastName}" : "Unknown"))
                .ForMember(dest => dest.DoctorSpecialization, opt => opt.MapFrom(src => src.Doctor != null ? src.Doctor.Specialization : ""));

            CreateMap<CreateAppointmentDto, Appointment>();
            CreateMap<UpdateAppointmentDto, Appointment>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // =================================================
            // 5. Consultations
            // =================================================
            CreateMap<Consultation, ConsultationDto>();
            CreateMap<CreateConsultationDto, Consultation>();
            CreateMap<UpdateConsultationDto, Consultation>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // =================================================
            // 6. Prescriptions
            // =================================================
            CreateMap<Prescription, PrescriptionDto>()
                // الوصول للطبيب عن طريق الاستشارة
                .ForMember(dest => dest.DoctorName, opt => opt.MapFrom(src =>
                    src.Consultation != null && src.Consultation.Doctor != null && src.Consultation.Doctor.User != null
                    ? $"{src.Consultation.Doctor.User.FirstName} {src.Consultation.Doctor.User.LastName}"
                    : "Unknown"))

                // الوصول للمريض عن طريق الاستشارة
                .ForMember(dest => dest.PatientName, opt => opt.MapFrom(src =>
                    src.Consultation != null && src.Consultation.Patient != null && src.Consultation.Patient.User != null
                    ? $"{src.Consultation.Patient.User.FirstName} {src.Consultation.Patient.User.LastName}"
                    : "Unknown"))

                // تعبئة الـ IDs أيضاً من الاستشارة
                .ForMember(dest => dest.DoctorId, opt => opt.MapFrom(src => src.Consultation != null ? src.Consultation.DoctorId : (int?)null))
                .ForMember(dest => dest.PatientId, opt => opt.MapFrom(src => src.Consultation != null ? src.Consultation.PatientId : (int?)null));

            CreateMap<CreatePrescriptionDto, Prescription>();
            CreateMap<UpdatePrescriptionDto, Prescription>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            // =================================================
            // 7. Medicine Requests
            // =================================================
            CreateMap<MedicineRequest, MedicineRequestDto>()
                .ForMember(dest => dest.PatientName, opt => opt.MapFrom(src => src.Patient != null ? $"{src.Patient.User.FirstName} {src.Patient.User.LastName}" : ""));

            CreateMap<CreateMedicineRequestDto, MedicineRequest>();
            CreateMap<UpdateMedicineRequestDto, MedicineRequest>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // =================================================
            // 8. Health Guides
            // =================================================
            CreateMap<HealthGuide, HealthGuideDto>()
                .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : "System"));

            CreateMap<CreateHealthGuideDto, HealthGuide>();
            CreateMap<UpdateHealthGuideDto, HealthGuide>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // =================================================
            // 9. Messages
            // =================================================
            CreateMap<Message, MessageDto>()
                .ForMember(dest => dest.SenderName, opt => opt.MapFrom(src => src.Sender != null ? $"{src.Sender.FirstName} {src.Sender.LastName}" : ""))
                .ForMember(dest => dest.ReceiverName, opt => opt.MapFrom(src => src.Receiver != null ? $"{src.Receiver.FirstName} {src.Receiver.LastName}" : ""));

            CreateMap<CreateMessageDto, Message>();

            // =================================================
            // 10. Mental Support Sessions
            // =================================================
            CreateMap<MentalSupportSession, MentalSupportSessionDto>()
                .ForMember(dest => dest.PatientName, opt => opt.MapFrom(src => src.Patient != null ? $"{src.Patient.User.FirstName} {src.Patient.User.LastName}" : ""))
                .ForMember(dest => dest.DoctorName, opt => opt.MapFrom(src => src.Doctor != null ? $"{src.Doctor.User.FirstName} {src.Doctor.User.LastName}" : ""));

            CreateMap<CreateMentalSupportSessionDto, MentalSupportSession>();
            CreateMap<UpdateMentalSupportSessionDto, MentalSupportSession>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // =================================================
            // 11. Notifications
            // =================================================
            CreateMap<Notification, NotificationDto>();
            CreateMap<CreateNotificationDto, Notification>();

            // =================================================
            // 12. Equipment
            // =================================================
            CreateMap<Equipment, EquipmentDto>();
            // إذا كان هناك علاقة مع NGO، يمكن إضافة Mapping للاسم
            // .ForMember(dest => dest.NGOName, opt => opt.MapFrom(src => src.NGO.OrganizationName));

            CreateMap<CreateEquipmentDto, Equipment>();
            CreateMap<UpdateEquipmentDto, Equipment>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // =================================================
            // 13. NGOs & Missions
            // =================================================
            CreateMap<NGO, NgoDto>();
            CreateMap<CreateNgoDto, NGO>();
            CreateMap<UpdateNgoDto, NGO>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<NgoMission, NgoMissionDto>()
                 .ForMember(dest => dest.NGOName, opt => opt.MapFrom(src => src.NGO != null ? src.NGO.OrganizationName : ""));

            CreateMap<CreateNgoMissionDto, NgoMission>();
            CreateMap<UpdateNgoMissionDto, NgoMission>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // =================================================
            // 14. Services
            // =================================================
            CreateMap<Service, ServiceDto>();
            CreateMap<CreateServiceDto, Service>();
            CreateMap<UpdateServiceDto, Service>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // =================================================
            // 15. Sponsorships
            // =================================================
            CreateMap<Sponsorship, SponsorshipDto>()
                .ForMember(dest => dest.PatientName, opt => opt.MapFrom(src => src.Patient != null ? $"{src.Patient.User.FirstName} {src.Patient.User.LastName}" : ""));

            CreateMap<CreateSponsorshipDto, Sponsorship>();
            CreateMap<UpdateSponsorshipDto, Sponsorship>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // =================================================
            // 16. Transactions
            // =================================================
            CreateMap<Transaction, TransactionDto>();
            CreateMap<CreateTransactionDto, Transaction>();
            CreateMap<UpdateTransactionDto, Transaction>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // =================================================
            // 17. Medical Facilities & Reviews
            // =================================================
            CreateMap<MedicalFacility, MedicalFacilityDto>();
            CreateMap<CreateMedicalFacilityDto, MedicalFacility>();
            CreateMap<UpdateMedicalFacilityDto, MedicalFacility>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<FacilityReview, FacilityReviewDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : ""));

            CreateMap<CreateFacilityReviewDto, FacilityReview>();

            // =================================================
            // 18. Public Alerts
            // =================================================
            CreateMap<PublicAlert, PublicAlertDto>()
                .ForMember(dest => dest.PostedBy, opt => opt.MapFrom(src => src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : "System"));

            CreateMap<CreatePublicAlertDto, PublicAlert>();
            CreateMap<UpdatePublicAlertDto, PublicAlert>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // =================================================
            // 19. Ratings
            // =================================================
            CreateMap<Rating, RatingDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : ""));

            CreateMap<CreateRatingDto, Rating>();
            CreateMap<UpdateRatingDto, Rating>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // =================================================
            // 20. Emergency & Locations
            // =================================================
            CreateMap<EmergencyCase, EmergencyCaseDto>()
                .ForMember(dest => dest.PatientName, opt => opt.MapFrom(src => src.Patient != null ? $"{src.Patient.User.FirstName} {src.Patient.User.LastName}" : ""));

            CreateMap<CreateEmergencyCaseDto, EmergencyCase>();

            CreateMap<UserLocation, UserLocationDto>();
            CreateMap<UpdateUserLocationDto, UserLocation>();

            CreateMap<EmergencyResponder, EmergencyResponderDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : ""));

            CreateMap<ServiceArea, ServiceAreaDto>();
            CreateMap<CreateServiceAreaDto, ServiceArea>();

            // =================================================
            // 21. Recommendations & Analytics
            // =================================================
            CreateMap<DoctorRecommendation, DoctorRecommendationDto>();
            CreateMap<PatientHealthProfile, PatientHealthProfileDto>();
            CreateMap<CreateHealthProfileDto, PatientHealthProfile>();
            CreateMap<UpdateHealthProfileDto, PatientHealthProfile>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<UserActivity, UserActivityDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : ""));

            CreateMap<LogUserActivityDto, UserActivity>();
        }
    }
}