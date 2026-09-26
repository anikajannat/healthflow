using Healthcare.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Healthcare.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<PatientProfile> PatientProfiles => Set<PatientProfile>();
    public DbSet<DoctorProfile> DoctorProfiles => Set<DoctorProfile>();
    public DbSet<DoctorSchedule> DoctorSchedules => Set<DoctorSchedule>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Prescription> Prescriptions => Set<Prescription>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<EmailOtp> EmailOtps => Set<EmailOtp>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<User>().HasIndex(x => x.Email).IsUnique();
        b.Entity<PatientProfile>().HasIndex(x => x.UserId).IsUnique();
        b.Entity<DoctorProfile>().HasIndex(x => x.UserId).IsUnique();
        b.Entity<DoctorProfile>().HasIndex(x => x.LicenseNumber).IsUnique();
        b.Entity<DoctorSchedule>().HasIndex(x => new { x.DoctorProfileId, x.Date }).IsUnique();
        b.Entity<Appointment>().HasIndex(x => new { x.ScheduleId, x.SlotIndex }).IsUnique();
        b.Entity<Appointment>().HasIndex(x => new { x.ScheduleId, x.SerialNumber }).IsUnique();
        b.Entity<Payment>().HasIndex(x => x.AppointmentId).IsUnique();
        b.Entity<Prescription>().HasIndex(x => x.AppointmentId).IsUnique();
        b.Entity<Review>().HasIndex(x => x.AppointmentId).IsUnique();

        b.Entity<DoctorProfile>().Property(x => x.ConsultationFee).HasColumnType("decimal(10,2)");
        b.Entity<Payment>().Property(x => x.Amount).HasColumnType("decimal(10,2)");

        b.Entity<User>()
            .HasOne(x => x.PatientProfile).WithOne(x => x.User)
            .HasForeignKey<PatientProfile>(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<User>()
            .HasOne(x => x.DoctorProfile).WithOne(x => x.User)
            .HasForeignKey<DoctorProfile>(x => x.UserId).OnDelete(DeleteBehavior.Cascade);

        b.Entity<Appointment>()
            .HasOne(x => x.PatientUser).WithMany()
            .HasForeignKey(x => x.PatientUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
