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

    // As propriedades das entidades permanecem em inglês; o mapeamento aponta para
    // o schema do banco em português (tabelas e colunas).
    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);

        // ── ApplicationUser → Usuarios ────────────────────────────────────────
        mb.Entity<ApplicationUser>(e =>
        {
            e.ToTable("Usuarios");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("IdUsuario");
            e.Property(x => x.FullName).HasColumnName("NomeCompleto").HasMaxLength(200).IsRequired();
            e.Property(x => x.Email).HasColumnName("Email").HasMaxLength(255).IsRequired(false);
            e.Property(x => x.PasswordHash).HasColumnName("SenhaHash");
            e.Property(x => x.Role).HasColumnName("Papel").HasMaxLength(20).HasDefaultValue("aluno");
            e.Property(x => x.Rgm).HasColumnName("Rgm").HasMaxLength(50);
            e.Property(x => x.Semester).HasColumnName("Semestre");
            e.Property(x => x.Shift).HasColumnName("Turno").HasMaxLength(10);
            e.Property(x => x.Phone).HasColumnName("Telefone").HasMaxLength(30);
            e.Property(x => x.Institution).HasColumnName("Instituicao").HasMaxLength(200);
            e.Property(x => x.MustChangePassword).HasColumnName("DeveTrocarSenha").HasDefaultValue(false);
            e.Property(x => x.MustSetEmail).HasColumnName("DeveDefinirEmail").HasDefaultValue(false);
            e.Property(x => x.IsActive).HasColumnName("Ativo").HasDefaultValue(true);
            e.Property(x => x.CreatedAt).HasColumnName("CriadoEm");
            e.Property(x => x.UpdatedAt).HasColumnName("AtualizadoEm");
            e.HasIndex(x => x.Email).IsUnique().HasFilter("\"Email\" IS NOT NULL");
            e.HasIndex(x => x.Rgm).IsUnique().HasFilter("\"Rgm\" IS NOT NULL");
        });

        // ── PasswordResetCode → CodigosRedefinicaoSenha ───────────────────────
        mb.Entity<PasswordResetCode>(e =>
        {
            e.ToTable("CodigosRedefinicaoSenha");
            e.HasKey(x => x.Id);
            e.Property(x => x.Email).HasColumnName("Email").HasMaxLength(255).IsRequired();
            e.Property(x => x.Code).HasColumnName("Codigo").HasMaxLength(6).IsRequired();
            e.Property(x => x.ExpiresAt).HasColumnName("ExpiraEm");
            e.Property(x => x.Used).HasColumnName("Usado");
            e.Property(x => x.CreatedAt).HasColumnName("CriadoEm");
            e.HasIndex(x => new { x.Email, x.Code });
        });

        // ── StudentSemesterHistory → HistoricoSemestreEstudante ───────────────
        mb.Entity<StudentSemesterHistory>(e =>
        {
            e.ToTable("HistoricoSemestreEstudante");
            e.HasKey(x => x.Id);
            e.Property(x => x.StudentId).HasColumnName("IdEstudante");
            e.Property(x => x.Semester).HasColumnName("Semestre");
            e.Property(x => x.TotalHours).HasColumnName("TotalHoras").HasColumnType("numeric(10,2)");
            e.Property(x => x.RecordedAt).HasColumnName("RegistradoEm");
            e.HasOne(x => x.Student)
             .WithMany()
             .HasForeignKey(x => x.StudentId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Location → Locais ─────────────────────────────────────────────────
        mb.Entity<Location>(e =>
        {
            e.ToTable("Locais");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("IdLocal");
            e.Property(x => x.Name).HasColumnName("Nome").HasMaxLength(300).IsRequired();
            e.Property(x => x.Address).HasColumnName("Endereco").HasMaxLength(500);
            e.Property(x => x.RadiusMeters).HasColumnName("RaioMetros");
            e.Property(x => x.IsInstitution).HasColumnName("EhInstituicao");
            e.Property(x => x.ShiftStart).HasColumnName("InicioTurno").HasMaxLength(5);
            e.Property(x => x.ShiftEnd).HasColumnName("FimTurno").HasMaxLength(5);
            e.Property(x => x.CodigoCnes).HasColumnName("CodigoCnes").HasMaxLength(20);
            e.Property(x => x.CreatedAt).HasColumnName("CriadoEm");
            e.HasIndex(x => x.CodigoCnes).IsUnique().HasFilter("\"CodigoCnes\" IS NOT NULL");
        });

        // ── StudentGroup → GruposEstudantes ───────────────────────────────────
        mb.Entity<StudentGroup>(e =>
        {
            e.ToTable("GruposEstudantes");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("IdGrupo");
            e.Property(x => x.Code).HasColumnName("Codigo").HasMaxLength(20).IsRequired();
            e.Property(x => x.Name).HasColumnName("Nome").HasMaxLength(200).IsRequired();
            e.Property(x => x.Description).HasColumnName("Descricao");
            e.Property(x => x.CreatedAt).HasColumnName("CriadoEm");
            e.HasIndex(x => x.Code).IsUnique();
        });

        // ── GroupMembership → MembrosGrupo ────────────────────────────────────
        mb.Entity<GroupMembership>(e =>
        {
            e.ToTable("MembrosGrupo");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("IdMembroGrupo");
            e.Property(x => x.StudentId).HasColumnName("IdEstudante");
            e.Property(x => x.GroupId).HasColumnName("IdGrupo");
            e.Property(x => x.CreatedAt).HasColumnName("CriadoEm");
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

        // ── RotationSchedule → EscalasRodizio ─────────────────────────────────
        mb.Entity<RotationSchedule>(e =>
        {
            e.ToTable("EscalasRodizio");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("IdEscala");
            e.Property(x => x.GroupId).HasColumnName("IdGrupo");
            e.Property(x => x.LocationId).HasColumnName("IdLocal");
            e.Property(x => x.PreceptorId).HasColumnName("IdPreceptor");
            e.Property(x => x.Shift).HasColumnName("Turno").HasMaxLength(10);
            e.Property(x => x.PeriodLabel).HasColumnName("RotuloPeriodo").HasMaxLength(100);
            e.Property(x => x.StartDate).HasColumnName("DataInicio");
            e.Property(x => x.EndDate).HasColumnName("DataFim");
            e.Property(x => x.ActivityType).HasColumnName("TipoAtividade").HasMaxLength(20);
            e.Property(x => x.RequiredHours).HasColumnName("HorasExigidas");
            e.Property(x => x.Notes).HasColumnName("Observacoes");
            e.Property(x => x.CreatedAt).HasColumnName("CriadoEm");
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

        // ── AttendanceRecord → RegistrosPresenca ──────────────────────────────
        mb.Entity<AttendanceRecord>(e =>
        {
            e.ToTable("RegistrosPresenca");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("IdPresenca");
            e.Property(x => x.StudentId).HasColumnName("IdEstudante");
            e.Property(x => x.ScheduleId).HasColumnName("IdEscala");
            e.Property(x => x.LocationId).HasColumnName("IdLocal");
            e.Property(x => x.Type).HasColumnName("Tipo").HasMaxLength(10);
            e.Property(x => x.RecordedAt).HasColumnName("RegistradoEm");
            e.Property(x => x.DistanceMeters).HasColumnName("DistanciaMetros");
            e.Property(x => x.PhotoUrl).HasColumnName("UrlFoto");
            e.Property(x => x.ActivitiesDescription).HasColumnName("DescricaoAtividades");
            e.Property(x => x.Status).HasColumnName("Status").HasMaxLength(15).HasDefaultValue("pendente");
            e.Property(x => x.IrregularityReason).HasColumnName("MotivoIrregularidade");
            e.Property(x => x.ValidatedById).HasColumnName("ValidadoPorId");
            e.Property(x => x.ValidatedAt).HasColumnName("ValidadoEm");
            e.Property(x => x.CreatedAt).HasColumnName("CriadoEm");
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

        // ── Evaluation → Avaliacoes ───────────────────────────────────────────
        mb.Entity<Evaluation>(e =>
        {
            e.ToTable("Avaliacoes");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("IdAvaliacao");
            e.Property(x => x.StudentId).HasColumnName("IdEstudante");
            e.Property(x => x.PreceptorId).HasColumnName("IdPreceptor");
            e.Property(x => x.ScheduleId).HasColumnName("IdEscala");
            e.Property(x => x.ActivitiesScore).HasColumnName("NotaAtividades");
            e.Property(x => x.PostureScore).HasColumnName("NotaPostura");
            e.Property(x => x.PlanningScore).HasColumnName("NotaPlanejamento");
            e.Property(x => x.Comment).HasColumnName("Comentario");
            e.Property(x => x.CreatedAt).HasColumnName("CriadoEm");
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

        // ── FormativeFollowup → AcompanhamentosFormativos ─────────────────────
        mb.Entity<FormativeFollowup>(e =>
        {
            e.ToTable("AcompanhamentosFormativos");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("IdAcompanhamento");
            e.Property(x => x.StudentId).HasColumnName("IdEstudante");
            e.Property(x => x.PreceptorId).HasColumnName("IdPreceptor");
            e.Property(x => x.ScheduleId).HasColumnName("IdEscala");
            e.Property(x => x.GroupId).HasColumnName("IdGrupo");
            e.Property(x => x.LocationId).HasColumnName("IdLocal");
            e.Property(x => x.Shift).HasColumnName("Turno");
            e.Property(x => x.PeriodLabel).HasColumnName("RotuloPeriodo");
            e.Property(x => x.Semester).HasColumnName("Semestre");
            e.Property(x => x.FollowUpStart).HasColumnName("InicioAcompanhamento");
            e.Property(x => x.FollowUpEnd).HasColumnName("FimAcompanhamento");
            // Dimensões comportamentais (nome de propriedade == nome de coluna).
            e.Property(x => x.Status).HasColumnName("Status").HasMaxLength(30).HasDefaultValue("rascunho");
            e.Property(x => x.PreceptorSignedAt).HasColumnName("AssinadoPreceptorEm");
            e.Property(x => x.PreceptorSignedName).HasColumnName("NomeAssinaturaPreceptor");
            e.Property(x => x.PreceptorSignedIp).HasColumnName("IpAssinaturaPreceptor");
            e.Property(x => x.PreceptorSignedUserId).HasColumnName("IdUsuarioAssinaturaPreceptor");
            e.Property(x => x.StudentSignedAt).HasColumnName("AssinadoEstudanteEm");
            e.Property(x => x.StudentSignedName).HasColumnName("NomeAssinaturaEstudante");
            e.Property(x => x.StudentSignedIp).HasColumnName("IpAssinaturaEstudante");
            e.Property(x => x.StudentSignedUserId).HasColumnName("IdUsuarioAssinaturaEstudante");
            e.Property(x => x.CreatedAt).HasColumnName("CriadoEm");
            e.Property(x => x.UpdatedAt).HasColumnName("AtualizadoEm");
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
