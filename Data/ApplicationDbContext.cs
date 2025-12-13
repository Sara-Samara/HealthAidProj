using HealthAidAPI.Models;
using HealthAidAPI.Models.Analytics;
using HealthAidAPI.Models.Emergency;
using HealthAidAPI.Models.Extras;
using HealthAidAPI.Models.Location;
using HealthAidAPI.Models.MedicalFacilities;
using HealthAidAPI.Models.Recommendations;
using HealthAidAPI.Models.Sync;
using Microsoft.EntityFrameworkCore;

namespace HealthAidAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        // =======================  
        // DbSets  
        // =======================
        public DbSet<User> Users { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Consultation> Consultations { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<MedicineRequest> MedicineRequests { get; set; }
        public DbSet<NGO> NGOs { get; set; }
        public DbSet<NgoMission> NgoMessions { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<Equipment> Equipments { get; set; }
        public DbSet<PublicAlert> PublicAlerts { get; set; }
        public DbSet<HealthGuide> HealthGuides { get; set; }
        public DbSet<MentalSupportSession> MentalSupportSessions { get; set; }
        public DbSet<Sponsorship> Sponsorships { get; set; }
        public DbSet<Donation> Donations { get; set; }
        public DbSet<Donor> Donors { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<EmergencyCase> EmergencyCases { get; set; }
        public DbSet<EmergencyResponder> EmergencyResponders { get; set; }
        public DbSet<EmergencyLog> EmergencyLogs { get; set; }
        public DbSet<MedicalFacility> MedicalFacilities { get; set; }
        public DbSet<FacilityReview> FacilityReviews { get; set; }
        public DbSet<SystemAnalytic> SystemAnalytics { get; set; }
        public DbSet<UserActivity> UserActivities { get; set; }
        public DbSet<PatientHealthProfile> PatientHealthProfiles { get; set; }
        public DbSet<DoctorRecommendation> DoctorRecommendations { get; set; }
        public DbSet<UserLocation> UserLocations { get; set; }
        public DbSet<ServiceArea> ServiceAreas { get; set; }
        public DbSet<OfflineQueue> OfflineQueues { get; set; }
        public DbSet<BloodRequest> BloodRequests { get; set; }
        public DbSet<PharmacyInventory> PharmacyInventories { get; set; }
        public DbSet<PatientVital> PatientVitals { get; set; }
        public DbSet<VolunteerOpportunity> VolunteerOpportunities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Precision configs
            modelBuilder.Entity<Consultation>().Property(c => c.Fee).HasPrecision(18, 2);
            modelBuilder.Entity<Donation>().Property(d => d.Amount).HasPrecision(18, 2);
            modelBuilder.Entity<Donor>().Property(d => d.TotalDonated).HasPrecision(18, 2);
            modelBuilder.Entity<Equipment>().Property(e => e.EstimatedValue).HasPrecision(18, 2);
            modelBuilder.Entity<NgoMission>().Property(n => n.Budget).HasPrecision(18, 2);
            modelBuilder.Entity<Sponsorship>().Property(s => s.AmountRaised).HasPrecision(18, 2);
            modelBuilder.Entity<Sponsorship>().Property(s => s.GoalAmount).HasPrecision(18, 2);
            modelBuilder.Entity<Service>().Property(s => s.Price).HasPrecision(18, 2);

            // =======================
            // User
            // =======================
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Email).IsUnique();
                entity.HasIndex(u => u.Phone).IsUnique();
                entity.Property(u => u.Role).HasDefaultValue("Patient");
                entity.Property(u => u.Status).HasDefaultValue("Active");
                entity.Property(u => u.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            // Doctor ↔ User (1:1)
            modelBuilder.Entity<Doctor>()
                .HasOne(d => d.User)
                .WithOne(u => u.Doctor)
                .HasForeignKey<Doctor>(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Patient ↔ User (1:1)
            modelBuilder.Entity<Patient>()
                .HasOne(p => p.User)
                .WithOne(u => u.Patient)
                .HasForeignKey<Patient>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Donor ↔ User (1:1)
            modelBuilder.Entity<Donor>()
                .HasOne(d => d.User)
                .WithOne(u => u.Donor)
                .HasForeignKey<Donor>(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // =======================
            // Messaging (Restrict)
            // =======================
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany(u => u.SentMessages)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Receiver)
                .WithMany(u => u.ReceivedMessages)
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            // =======================
            // Notifications
            // =======================
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Sender)
                .WithMany(u => u.SentNotifications)
                .HasForeignKey(n => n.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Receiver)
                .WithMany(u => u.ReceivedNotifications)
                .HasForeignKey(n => n.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            // =======================
            // Appointments  
            // =======================
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Doctor)
                .WithMany(d => d.Appointments)
                .HasForeignKey(a => a.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Patient)
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            // =======================
            // Consultations
            // =======================
            modelBuilder.Entity<Consultation>()
                .HasOne(c => c.Doctor)
                .WithMany(d => d.Consultations)
                .HasForeignKey(c => c.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Consultation>()
                .HasOne(c => c.Patient)
                .WithMany(p => p.Consultations)
                .HasForeignKey(c => c.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Consultation>()
                .HasOne(c => c.Appointment)
                .WithMany(a => a.Consultations)
                .HasForeignKey(c => c.AppointmentId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            // =======================
            // Prescription
            // =======================
            modelBuilder.Entity<Prescription>()
                .HasOne(p => p.Consultation)
                .WithMany(c => c.Prescriptions)
                .HasForeignKey(p => p.ConsultationId)
                .OnDelete(DeleteBehavior.Cascade);

            // =======================
            // Medicine Request
            // =======================
            modelBuilder.Entity<MedicineRequest>()
                .HasOne(mr => mr.Patient)
                .WithMany(p => p.MedicineRequests)
                .HasForeignKey(mr => mr.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            // =======================
            // Sponsorship
            // =======================
            modelBuilder.Entity<Sponsorship>()
                .HasOne(s => s.Patient)
                .WithMany(p => p.Sponsorships)
                .HasForeignKey(s => s.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            // =======================
            // Donations
            // =======================
            modelBuilder.Entity<Donation>()
                .HasOne(d => d.Sponsorship)
                .WithMany(s => s.Donations)
                .HasForeignKey(d => d.SponsorshipId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Donation>()
                .HasOne(d => d.Donor)
                .WithMany(dn => dn.Donations)
                .HasForeignKey(d => d.DonorId)
                .OnDelete(DeleteBehavior.Restrict);

            // =======================
            // Mental Support Sessions
            // =======================
            modelBuilder.Entity<MentalSupportSession>()
                .HasOne(m => m.Doctor)
                .WithMany(d => d.MentalSupportSessions)
                .HasForeignKey(m => m.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MentalSupportSession>()
                .HasOne(m => m.Patient)
                .WithMany(p => p.MentalSupportSessions)
                .HasForeignKey(m => m.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            // =======================
            // NGO Missions
            // =======================
            modelBuilder.Entity<NgoMission>()
                .HasOne(nm => nm.NGO)
                .WithMany(n => n.NgoMessions)
                .HasForeignKey(nm => nm.NGOId)
                .OnDelete(DeleteBehavior.Cascade);

            // Equipment
            modelBuilder.Entity<Equipment>()
                .HasOne(e => e.NGO)
                .WithMany(n => n.Equipments)
                .HasForeignKey(e => e.NGOId)
                .OnDelete(DeleteBehavior.Cascade);

            // Health Guides
            modelBuilder.Entity<HealthGuide>()
                .HasOne(hg => hg.User)
                .WithMany(u => u.HealthGuides)
                .HasForeignKey(hg => hg.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Public Alert
            modelBuilder.Entity<PublicAlert>()
                .HasOne(pa => pa.User)
                .WithMany(u => u.PublicAlerts)
                .HasForeignKey(pa => pa.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ratings
            modelBuilder.Entity<Rating>()
                .HasOne(r => r.User)
                .WithMany(u => u.Ratings)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Transactions
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.User)
                .WithMany(u => u.Transactions)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Consultation)
                .WithMany(c => c.Transactions)
                .HasForeignKey(t => t.ConsultationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Donation)
                .WithMany(d => d.Transactions)
                .HasForeignKey(t => t.DonationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.MedicineRequest)
                .WithMany(mr => mr.Transactions)
                .HasForeignKey(t => t.MedicineRequestId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            // =====================================
            // Emergency Cases
            // =====================================
            modelBuilder.Entity<EmergencyCase>(entity =>
            {
                entity.HasOne(ec => ec.Patient)
                    .WithMany()
                    .HasForeignKey(ec => ec.PatientId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(ec => ec.Doctor)
                    .WithMany()
                    .HasForeignKey(ec => ec.AssignedDoctorId)
                    .OnDelete(DeleteBehavior.SetNull);

                 entity.HasOne(ec => ec.Responder)
                                    .WithMany(er => er.EmergencyCases)
                                    .HasForeignKey(ec => ec.ResponderId)
                                    .OnDelete(DeleteBehavior.Restrict);


                entity.HasMany(ec => ec.EmergencyLogs)
                    .WithOne(el => el.EmergencyCase)
                    .HasForeignKey(el => el.EmergencyCaseId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Emergency Responders
            modelBuilder.Entity<EmergencyResponder>()
                .HasOne(er => er.User)
                .WithMany()
                .HasForeignKey(er => er.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EmergencyLog>()
                .HasOne(el => el.User)
                .WithMany()
                .HasForeignKey(el => el.PerformedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // =======================
            // Medical Facilities
            // =======================
            modelBuilder.Entity<MedicalFacility>()
                .HasMany(mf => mf.Reviews)
                .WithOne(fr => fr.Facility)
                .HasForeignKey(fr => fr.FacilityId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FacilityReview>()
                .HasOne(fr => fr.User)
                .WithMany()
                .HasForeignKey(fr => fr.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // =======================
            // Analytics
            // =======================
            modelBuilder.Entity<UserActivity>()
                .HasOne(ua => ua.User)
                .WithMany()
                .HasForeignKey(ua => ua.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Recommendations
            modelBuilder.Entity<PatientHealthProfile>()
                .HasOne(php => php.Patient)
                .WithMany()
                .HasForeignKey(php => php.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DoctorRecommendation>()
                .HasOne(dr => dr.Patient)
                .WithMany()
                .HasForeignKey(dr => dr.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DoctorRecommendation>()
                .HasOne(dr => dr.Doctor)
                .WithMany()
                .HasForeignKey(dr => dr.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Location
            modelBuilder.Entity<UserLocation>()
                .HasOne(ul => ul.User)
                .WithMany()
                .HasForeignKey(ul => ul.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Offline Sync
            modelBuilder.Entity<OfflineQueue>()
                .HasOne(oq => oq.User)
                .WithMany()
                .HasForeignKey(oq => oq.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
