using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EstagioCheck.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false),
                    RadiusMeters = table.Column<int>(type: "integer", nullable: false),
                    IsInstitution = table.Column<bool>(type: "boolean", nullable: false),
                    ShiftStart = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    ShiftEnd = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StudentGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "aluno"),
                    Matricula = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GroupMemberships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    GroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupMemberships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupMemberships_StudentGroups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "StudentGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupMemberships_Users_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RotationSchedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    LocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreceptorId = table.Column<Guid>(type: "uuid", nullable: true),
                    Shift = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    PeriodLabel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ActivityType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RequiredHours = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RotationSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RotationSchedules_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RotationSchedules_StudentGroups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "StudentGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RotationSchedules_Users_PreceptorId",
                        column: x => x.PreceptorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "AttendanceRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScheduleId = table.Column<Guid>(type: "uuid", nullable: true),
                    LocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    Type = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false),
                    DistanceMeters = table.Column<double>(type: "double precision", nullable: true),
                    PhotoUrl = table.Column<string>(type: "text", nullable: true),
                    ActivitiesDescription = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, defaultValue: "pendente"),
                    IrregularityReason = table.Column<string>(type: "text", nullable: true),
                    ValidatedById = table.Column<Guid>(type: "uuid", nullable: true),
                    ValidatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttendanceRecords_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AttendanceRecords_RotationSchedules_ScheduleId",
                        column: x => x.ScheduleId,
                        principalTable: "RotationSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AttendanceRecords_Users_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AttendanceRecords_Users_ValidatedById",
                        column: x => x.ValidatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Evaluations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreceptorId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScheduleId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActivitiesScore = table.Column<short>(type: "smallint", nullable: false),
                    PostureScore = table.Column<short>(type: "smallint", nullable: false),
                    PlanningScore = table.Column<short>(type: "smallint", nullable: false),
                    Comment = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Evaluations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Evaluations_RotationSchedules_ScheduleId",
                        column: x => x.ScheduleId,
                        principalTable: "RotationSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Evaluations_Users_PreceptorId",
                        column: x => x.PreceptorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Evaluations_Users_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FormativeFollowups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreceptorId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScheduleId = table.Column<Guid>(type: "uuid", nullable: true),
                    GroupId = table.Column<Guid>(type: "uuid", nullable: true),
                    LocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    Shift = table.Column<string>(type: "text", nullable: true),
                    PeriodLabel = table.Column<string>(type: "text", nullable: true),
                    Semester = table.Column<string>(type: "text", nullable: true),
                    FollowUpStart = table.Column<DateOnly>(type: "date", nullable: true),
                    FollowUpEnd = table.Column<DateOnly>(type: "date", nullable: true),
                    PosturaPontualidade = table.Column<string>(type: "text", nullable: true),
                    PosturaEtica = table.Column<string>(type: "text", nullable: true),
                    PosturaResponsabilidade = table.Column<string>(type: "text", nullable: true),
                    ComunicacaoEquipe = table.Column<string>(type: "text", nullable: true),
                    ComunicacaoPaciente = table.Column<string>(type: "text", nullable: true),
                    ComunicacaoEscuta = table.Column<string>(type: "text", nullable: true),
                    OrganizacaoPlanejamento = table.Column<string>(type: "text", nullable: true),
                    OrganizacaoSeguranca = table.Column<string>(type: "text", nullable: true),
                    OrganizacaoRegistros = table.Column<string>(type: "text", nullable: true),
                    ParticipacaoIniciativa = table.Column<string>(type: "text", nullable: true),
                    ParticipacaoAprendizado = table.Column<string>(type: "text", nullable: true),
                    ParticipacaoAutocritica = table.Column<string>(type: "text", nullable: true),
                    Potencialidades = table.Column<string>(type: "text", nullable: true),
                    AspectosAprimorar = table.Column<string>(type: "text", nullable: true),
                    SituacoesRelevantes = table.Column<string>(type: "text", nullable: true),
                    ObservacoesDocente = table.Column<string>(type: "text", nullable: true),
                    EvolucaoSemanal = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "rascunho"),
                    PreceptorSignedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    PreceptorSignedName = table.Column<string>(type: "text", nullable: true),
                    PreceptorSignedIp = table.Column<string>(type: "text", nullable: true),
                    PreceptorSignedUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    StudentSignedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    StudentSignedName = table.Column<string>(type: "text", nullable: true),
                    StudentSignedIp = table.Column<string>(type: "text", nullable: true),
                    StudentSignedUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormativeFollowups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormativeFollowups_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_FormativeFollowups_RotationSchedules_ScheduleId",
                        column: x => x.ScheduleId,
                        principalTable: "RotationSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_FormativeFollowups_StudentGroups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "StudentGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_FormativeFollowups_Users_PreceptorId",
                        column: x => x.PreceptorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FormativeFollowups_Users_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRecords_LocationId",
                table: "AttendanceRecords",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRecords_ScheduleId",
                table: "AttendanceRecords",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRecords_StudentId",
                table: "AttendanceRecords",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRecords_ValidatedById",
                table: "AttendanceRecords",
                column: "ValidatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Evaluations_PreceptorId",
                table: "Evaluations",
                column: "PreceptorId");

            migrationBuilder.CreateIndex(
                name: "IX_Evaluations_ScheduleId",
                table: "Evaluations",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_Evaluations_StudentId",
                table: "Evaluations",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_FormativeFollowups_GroupId",
                table: "FormativeFollowups",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_FormativeFollowups_LocationId",
                table: "FormativeFollowups",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_FormativeFollowups_PreceptorId",
                table: "FormativeFollowups",
                column: "PreceptorId");

            migrationBuilder.CreateIndex(
                name: "IX_FormativeFollowups_ScheduleId",
                table: "FormativeFollowups",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_FormativeFollowups_StudentId",
                table: "FormativeFollowups",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupMemberships_GroupId",
                table: "GroupMemberships",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupMemberships_StudentId",
                table: "GroupMemberships",
                column: "StudentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RotationSchedules_GroupId",
                table: "RotationSchedules",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_RotationSchedules_LocationId",
                table: "RotationSchedules",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_RotationSchedules_PreceptorId",
                table: "RotationSchedules",
                column: "PreceptorId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentGroups_Code",
                table: "StudentGroups",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AttendanceRecords");

            migrationBuilder.DropTable(
                name: "Evaluations");

            migrationBuilder.DropTable(
                name: "FormativeFollowups");

            migrationBuilder.DropTable(
                name: "GroupMemberships");

            migrationBuilder.DropTable(
                name: "RotationSchedules");

            migrationBuilder.DropTable(
                name: "Locations");

            migrationBuilder.DropTable(
                name: "StudentGroups");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
