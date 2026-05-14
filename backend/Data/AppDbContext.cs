using EstagioCheck.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EstagioCheck.API.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<ApplicationUser> Users => Set<ApplicationUser>();
    public DbSet<PasswordResetCode> PasswordResetCodes => Set<PasswordResetCode>();
    public DbSet<StudentSemesterHistory> StudentSemesterHistories => Set<StudentSemesterHistory>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<StudentGroup> StudentGroups => Set<StudentGroup>();
    public DbSet<GroupMembership> GroupMemberships => Set<GroupMembership>();
    public DbSet<RotationSchedule> RotationSchedules => Set<RotationSchedule>();
    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();
    public DbSet<Evaluation> Evaluations => Set<Evaluation>();
    public DbSet<FormativeFollowup> FormativeFollowups => Set<FormativeFollowup>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);

        // ── ApplicationUser ───────────────────────────────────────────────────
        mb.Entity<ApplicationUser>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Email).IsUnique().HasFilter("email IS NOT NULL");
            e.HasIndex(x => x.Rgm).IsUnique().HasFilter("rgm IS NOT NULL");
            e.Property(x => x.Role).HasMaxLength(20).HasDefaultValue("aluno");
            e.Property(x => x.FullName).HasMaxLength(200).IsRequired();
            e.Property(x => x.Email).HasMaxLength(255).IsRequired(false);
            e.Property(x => x.Matricula).HasMaxLength(50);
            e.Property(x => x.Rgm).HasMaxLength(50);
            e.Property(x => x.Shift).HasMaxLength(10);
            e.Property(x => x.Phone).HasMaxLength(30);
            e.Property(x => x.Institution).HasMaxLength(200);
            e.Property(x => x.MustChangePassword).HasDefaultValue(false);
            e.Property(x => x.MustSetEmail).HasDefaultValue(false);
            e.Property(x => x.IsActive).HasDefaultValue(true);
        });

        // ── PasswordResetCode ─────────────────────────────────────────────────
        mb.Entity<PasswordResetCode>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.Email, x.Code });
            e.Property(x => x.Email).HasMaxLength(255).IsRequired();
            e.Property(x => x.Code).HasMaxLength(6).IsRequired();
        });

        // ── StudentSemesterHistory ────────────────────────────────────────────
        mb.Entity<StudentSemesterHistory>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Student)
             .WithMany()
             .HasForeignKey(x => x.StudentId)
             .OnDelete(DeleteBehavior.Cascade);
            e.Property(x => x.TotalHours).HasColumnType("numeric(10,2)");
        });

        // ── Location ──────────────────────────────────────────────────────────
        mb.Entity<Location>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.CodigoCnes).IsUnique().HasFilter("\"CodigoCnes\" IS NOT NULL");
            e.Property(x => x.Name).HasMaxLength(300).IsRequired();
            e.Property(x => x.Address).HasMaxLength(500);
            e.Property(x => x.ShiftStart).HasMaxLength(5);
            e.Property(x => x.ShiftEnd).HasMaxLength(5);
            e.Property(x => x.CodigoCnes).HasMaxLength(20);
        });

        // ── StudentGroup ──────────────────────────────────────────────────────
        mb.Entity<StudentGroup>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Code).IsUnique();
            e.Property(x => x.Code).HasMaxLength(20).IsRequired();
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
        });

        // ── GroupMembership ───────────────────────────────────────────────────
        mb.Entity<GroupMembership>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.StudentId).IsUnique(); // 1 aluno → 1 grupo
            e.HasOne(x => x.Student)
             .WithOne(u => u.GroupMembership)
             .HasForeignKey<GroupMembership>(x => x.StudentId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Group)
             .WithMany(g => g.Memberships)
             .HasForeignKey(x => x.GroupId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── RotationSchedule ──────────────────────────────────────────────────
        mb.Entity<RotationSchedule>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Shift).HasMaxLength(10);
            e.Property(x => x.ActivityType).HasMaxLength(20);
            e.Property(x => x.PeriodLabel).HasMaxLength(100);
            e.HasOne(x => x.Group)
             .WithMany(g => g.Schedules)
             .HasForeignKey(x => x.GroupId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Location)
             .WithMany(l => l.Schedules)
             .HasForeignKey(x => x.LocationId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Preceptor)
             .WithMany(u => u.SchedulesAsPreceptor)
             .HasForeignKey(x => x.PreceptorId)
             .OnDelete(DeleteBehavior.SetNull)
             .IsRequired(false);
        });

        // ── AttendanceRecord ──────────────────────────────────────────────────
        mb.Entity<AttendanceRecord>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Type).HasMaxLength(10);
            e.Property(x => x.Status).HasMaxLength(15).HasDefaultValue("pendente");
            e.HasOne(x => x.Student)
             .WithMany(u => u.AttendanceRecords)
             .HasForeignKey(x => x.StudentId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Schedule)
             .WithMany(s => s.AttendanceRecords)
             .HasForeignKey(x => x.ScheduleId)
             .OnDelete(DeleteBehavior.SetNull)
             .IsRequired(false);
            e.HasOne(x => x.Location)
             .WithMany(l => l.AttendanceRecords)
             .HasForeignKey(x => x.LocationId)
             .OnDelete(DeleteBehavior.SetNull)
             .IsRequired(false);
            e.HasOne(x => x.ValidatedBy)
             .WithMany()
             .HasForeignKey(x => x.ValidatedById)
             .OnDelete(DeleteBehavior.SetNull)
             .IsRequired(false);
        });

        // ── Evaluation ────────────────────────────────────────────────────────
        mb.Entity<Evaluation>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Student)
             .WithMany(u => u.EvaluationsAsStudent)
             .HasForeignKey(x => x.StudentId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Preceptor)
             .WithMany(u => u.EvaluationsAsPreceptor)
             .HasForeignKey(x => x.PreceptorId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Schedule)
             .WithMany()
             .HasForeignKey(x => x.ScheduleId)
             .OnDelete(DeleteBehavior.SetNull)
             .IsRequired(false);
        });

        // ── FormativeFollowup ─────────────────────────────────────────────────
        mb.Entity<FormativeFollowup>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Status).HasMaxLength(30).HasDefaultValue("rascunho");
            e.HasOne(x => x.Student)
             .WithMany(u => u.FollowupsAsStudent)
             .HasForeignKey(x => x.StudentId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Preceptor)
             .WithMany(u => u.FollowupsAsPreceptor)
             .HasForeignKey(x => x.PreceptorId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Schedule)
             .WithMany(s => s.Followups)
             .HasForeignKey(x => x.ScheduleId)
             .OnDelete(DeleteBehavior.SetNull)
             .IsRequired(false);
            e.HasOne(x => x.Group)
             .WithMany()
             .HasForeignKey(x => x.GroupId)
             .OnDelete(DeleteBehavior.SetNull)
             .IsRequired(false);
            e.HasOne(x => x.Location)
             .WithMany()
             .HasForeignKey(x => x.LocationId)
             .OnDelete(DeleteBehavior.SetNull)
             .IsRequired(false);
        });
    }
}
