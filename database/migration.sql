-- EstagioCheck - Script PostgreSQL para Supabase
-- Gerado para substituir o script T-SQL (SQL Server) original.
-- Execute no SQL Editor do Supabase ou via psql caso prefira
-- criar as tabelas manualmente (sem rodar as migrations do EF Core).
--
-- Convenções:
--   • Identificadores entre aspas duplas preservam o case (PascalCase),
--     compatível com o que o EF Core / Npgsql gera por padrão.
--   • UUID como tipo de chave primária (equivale a UNIQUEIDENTIFIER).
--   • gen_random_uuid() disponível no PostgreSQL 13+ sem extensão.
--   • TIMESTAMPTZ = TIMESTAMP WITH TIME ZONE (equivale a DATETIMEOFFSET).
-- ---------------------------------------------------------------------------

CREATE TABLE "Users" (
    "Id"           UUID         NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    "FullName"     VARCHAR(200) NOT NULL,
    "Email"        VARCHAR(255) NOT NULL,
    "PasswordHash" TEXT         NOT NULL,
    "Role"         VARCHAR(20)  NOT NULL DEFAULT 'aluno',
    "Matricula"    VARCHAR(50)  NULL,
    "Phone"        VARCHAR(30)  NULL,
    "CreatedAt"    TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    "UpdatedAt"    TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    CONSTRAINT "UQ_Users_Email" UNIQUE ("Email")
);

CREATE TABLE "Locations" (
    "Id"            UUID             NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    "Name"          VARCHAR(300)     NOT NULL,
    "Address"       VARCHAR(500)     NULL,
    "Latitude"      DOUBLE PRECISION NOT NULL DEFAULT 0,
    "Longitude"     DOUBLE PRECISION NOT NULL DEFAULT 0,
    "RadiusMeters"  INTEGER          NOT NULL DEFAULT 100,
    "IsInstitution" BOOLEAN          NOT NULL DEFAULT TRUE,
    "ShiftStart"    VARCHAR(5)       NOT NULL DEFAULT '07:00',
    "ShiftEnd"      VARCHAR(5)       NOT NULL DEFAULT '13:00',
    "CreatedAt"     TIMESTAMPTZ      NOT NULL DEFAULT NOW()
);

CREATE TABLE "StudentGroups" (
    "Id"          UUID         NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    "Code"        VARCHAR(20)  NOT NULL,
    "Name"        VARCHAR(200) NOT NULL,
    "Description" TEXT         NULL,
    "CreatedAt"   TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    CONSTRAINT "UQ_Groups_Code" UNIQUE ("Code")
);

CREATE TABLE "GroupMemberships" (
    "Id"        UUID        NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    "StudentId" UUID        NOT NULL,
    "GroupId"   UUID        NOT NULL,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT "UQ_GroupMembership_Student" UNIQUE ("StudentId"),
    CONSTRAINT "FK_GroupMemberships_Student" FOREIGN KEY ("StudentId")
        REFERENCES "Users"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_GroupMemberships_Group" FOREIGN KEY ("GroupId")
        REFERENCES "StudentGroups"("Id") ON DELETE CASCADE
);

CREATE TABLE "RotationSchedules" (
    "Id"            UUID             NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    "GroupId"       UUID             NOT NULL,
    "LocationId"    UUID             NOT NULL,
    "PreceptorId"   UUID             NULL,
    "Shift"         VARCHAR(10)      NOT NULL,
    "PeriodLabel"   VARCHAR(100)     NULL,
    "StartDate"     DATE             NOT NULL,
    "EndDate"       DATE             NOT NULL,
    "ActivityType"  VARCHAR(20)      NOT NULL DEFAULT 'assistencia',
    "RequiredHours" DOUBLE PRECISION NOT NULL DEFAULT 0,
    "Notes"         TEXT             NULL,
    CONSTRAINT "FK_RotationSchedules_Group" FOREIGN KEY ("GroupId")
        REFERENCES "StudentGroups"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_RotationSchedules_Location" FOREIGN KEY ("LocationId")
        REFERENCES "Locations"("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_RotationSchedules_Preceptor" FOREIGN KEY ("PreceptorId")
        REFERENCES "Users"("Id") ON DELETE SET NULL
);

CREATE TABLE "AttendanceRecords" (
    "Id"                    UUID             NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    "StudentId"             UUID             NOT NULL,
    "ScheduleId"            UUID             NULL,
    "LocationId"            UUID             NULL,
    "Type"                  VARCHAR(10)      NOT NULL DEFAULT 'check_in',
    "RecordedAt"            TIMESTAMPTZ      NOT NULL DEFAULT NOW(),
    "Latitude"              DOUBLE PRECISION NOT NULL DEFAULT 0,
    "Longitude"             DOUBLE PRECISION NOT NULL DEFAULT 0,
    "DistanceMeters"        DOUBLE PRECISION NULL,
    "PhotoUrl"              TEXT             NULL,
    "ActivitiesDescription" TEXT             NULL,
    "Status"                VARCHAR(15)      NOT NULL DEFAULT 'pendente',
    "IrregularityReason"    TEXT             NULL,
    "ValidatedById"         UUID             NULL,
    "ValidatedAt"           TIMESTAMPTZ      NULL,
    "CreatedAt"             TIMESTAMPTZ      NOT NULL DEFAULT NOW(),
    CONSTRAINT "FK_AttendanceRecords_Student" FOREIGN KEY ("StudentId")
        REFERENCES "Users"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_AttendanceRecords_Schedule" FOREIGN KEY ("ScheduleId")
        REFERENCES "RotationSchedules"("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_AttendanceRecords_Location" FOREIGN KEY ("LocationId")
        REFERENCES "Locations"("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_AttendanceRecords_Validator" FOREIGN KEY ("ValidatedById")
        REFERENCES "Users"("Id") ON DELETE SET NULL
);

CREATE TABLE "Evaluations" (
    "Id"              UUID        NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    "StudentId"       UUID        NOT NULL,
    "PreceptorId"     UUID        NOT NULL,
    "ScheduleId"      UUID        NULL,
    "ActivitiesScore" INTEGER     NOT NULL,
    "PostureScore"    INTEGER     NOT NULL,
    "PlanningScore"   INTEGER     NOT NULL,
    "Comment"         TEXT        NULL,
    "CreatedAt"       TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT "FK_Evaluations_Student" FOREIGN KEY ("StudentId")
        REFERENCES "Users"("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Evaluations_Preceptor" FOREIGN KEY ("PreceptorId")
        REFERENCES "Users"("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Evaluations_Schedule" FOREIGN KEY ("ScheduleId")
        REFERENCES "RotationSchedules"("Id") ON DELETE SET NULL
);

CREATE TABLE "FormativeFollowups" (
    "Id"          UUID        NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    "StudentId"   UUID        NOT NULL,
    "PreceptorId" UUID        NOT NULL,
    "ScheduleId"  UUID        NULL,
    "GroupId"     UUID        NULL,
    "LocationId"  UUID        NULL,
    "Shift"       TEXT        NULL,
    "PeriodLabel" VARCHAR(100) NULL,
    "Semester"    TEXT        NULL,
    "FollowUpStart" DATE      NULL,
    "FollowUpEnd"   DATE      NULL,
    -- Postura profissional e ética
    "PosturaPontualidade"     TEXT NULL,
    "PosturaEtica"            TEXT NULL,
    "PosturaResponsabilidade" TEXT NULL,
    -- Comunicação e trabalho em equipe
    "ComunicacaoEquipe"   TEXT NULL,
    "ComunicacaoPaciente" TEXT NULL,
    "ComunicacaoEscuta"   TEXT NULL,
    -- Organização e segurança no cuidado
    "OrganizacaoPlanejamento" TEXT NULL,
    "OrganizacaoSeguranca"    TEXT NULL,
    "OrganizacaoRegistros"    TEXT NULL,
    -- Participação e desenvolvimento
    "ParticipacaoIniciativa"  TEXT NULL,
    "ParticipacaoAprendizado" TEXT NULL,
    "ParticipacaoAutocritica" TEXT NULL,
    -- Campos descritivos
    "Potencialidades"      TEXT NULL,
    "AspectosAprimorar"    TEXT NULL,
    "SituacoesRelevantes"  TEXT NULL,
    "ObservacoesDocente"   TEXT NULL,
    "EvolucaoSemanal"      TEXT NULL,
    -- Status
    "Status" VARCHAR(30) NOT NULL DEFAULT 'rascunho',
    -- Assinatura do preceptor
    "PreceptorSignedAt"     TIMESTAMPTZ  NULL,
    "PreceptorSignedName"   TEXT         NULL,
    "PreceptorSignedIp"     TEXT         NULL,
    "PreceptorSignedUserId" UUID         NULL,
    -- Assinatura do aluno
    "StudentSignedAt"     TIMESTAMPTZ  NULL,
    "StudentSignedName"   TEXT         NULL,
    "StudentSignedIp"     TEXT         NULL,
    "StudentSignedUserId" UUID         NULL,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT "FK_FormativeFollowups_Student" FOREIGN KEY ("StudentId")
        REFERENCES "Users"("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_FormativeFollowups_Preceptor" FOREIGN KEY ("PreceptorId")
        REFERENCES "Users"("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_FormativeFollowups_Schedule" FOREIGN KEY ("ScheduleId")
        REFERENCES "RotationSchedules"("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_FormativeFollowups_Group" FOREIGN KEY ("GroupId")
        REFERENCES "StudentGroups"("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_FormativeFollowups_Location" FOREIGN KEY ("LocationId")
        REFERENCES "Locations"("Id") ON DELETE SET NULL
);

-- ---------------------------------------------------------------------------
-- Índices adicionais (além das PKs e UNIQUE já criados acima)
-- ---------------------------------------------------------------------------
CREATE INDEX "IX_GroupMemberships_GroupId"
    ON "GroupMemberships" ("GroupId");

CREATE INDEX "IX_RotationSchedules_GroupId"
    ON "RotationSchedules" ("GroupId");
CREATE INDEX "IX_RotationSchedules_LocationId"
    ON "RotationSchedules" ("LocationId");
CREATE INDEX "IX_RotationSchedules_PreceptorId"
    ON "RotationSchedules" ("PreceptorId");

CREATE INDEX "IX_AttendanceRecords_StudentId"
    ON "AttendanceRecords" ("StudentId");
CREATE INDEX "IX_AttendanceRecords_ScheduleId"
    ON "AttendanceRecords" ("ScheduleId");
CREATE INDEX "IX_AttendanceRecords_LocationId"
    ON "AttendanceRecords" ("LocationId");
CREATE INDEX "IX_AttendanceRecords_RecordedAt"
    ON "AttendanceRecords" ("RecordedAt" DESC);

CREATE INDEX "IX_Evaluations_StudentId"
    ON "Evaluations" ("StudentId");
CREATE INDEX "IX_Evaluations_PreceptorId"
    ON "Evaluations" ("PreceptorId");

CREATE INDEX "IX_FormativeFollowups_StudentId"
    ON "FormativeFollowups" ("StudentId");
CREATE INDEX "IX_FormativeFollowups_PreceptorId"
    ON "FormativeFollowups" ("PreceptorId");


CREATE TABLE ApplicationUsers (
    Id              UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID() PRIMARY KEY,
    FullName        NVARCHAR(200)    NOT NULL,
    Email           NVARCHAR(256)    NOT NULL,
    PasswordHash    NVARCHAR(500)    NOT NULL,
    Role            NVARCHAR(50)     NOT NULL DEFAULT 'aluno',
    Matricula       NVARCHAR(100)    NULL,
    Phone           NVARCHAR(50)     NULL,
    CreatedAt       DATETIMEOFFSET   NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    UpdatedAt       DATETIMEOFFSET   NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT UQ_Users_Email UNIQUE (Email)
);

CREATE TABLE Locations (
    Id              UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID() PRIMARY KEY,
    Name            NVARCHAR(200)    NOT NULL,
    Address         NVARCHAR(500)    NULL,
    Latitude        FLOAT            NOT NULL,
    Longitude       FLOAT            NOT NULL,
    RadiusMeters    FLOAT            NOT NULL DEFAULT 100,
    IsInstitution   BIT              NOT NULL DEFAULT 1,
    ShiftStart      NVARCHAR(10)     NULL,
    ShiftEnd        NVARCHAR(10)     NULL,
    CreatedAt       DATETIMEOFFSET   NOT NULL DEFAULT SYSDATETIMEOFFSET()
);

CREATE TABLE StudentGroups (
    Id              UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID() PRIMARY KEY,
    Code            NVARCHAR(50)     NOT NULL,
    Name            NVARCHAR(200)    NOT NULL,
    Description     NVARCHAR(500)    NULL,
    CreatedAt       DATETIMEOFFSET   NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT UQ_Groups_Code UNIQUE (Code)
);

CREATE TABLE GroupMemberships (
    Id              UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID() PRIMARY KEY,
    StudentId       UNIQUEIDENTIFIER NOT NULL,
    GroupId         UNIQUEIDENTIFIER NOT NULL,
    CreatedAt       DATETIMEOFFSET   NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT UQ_GroupMembership_Student UNIQUE (StudentId),
    CONSTRAINT FK_GroupMemberships_Student FOREIGN KEY (StudentId) REFERENCES ApplicationUsers(Id) ON DELETE CASCADE,
    CONSTRAINT FK_GroupMemberships_Group  FOREIGN KEY (GroupId)   REFERENCES StudentGroups(Id)    ON DELETE CASCADE
);

CREATE TABLE RotationSchedules (
    Id              UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID() PRIMARY KEY,
    GroupId         UNIQUEIDENTIFIER NOT NULL,
    LocationId      UNIQUEIDENTIFIER NOT NULL,
    PreceptorId     UNIQUEIDENTIFIER NULL,
    Shift           NVARCHAR(20)     NOT NULL,
    PeriodLabel     NVARCHAR(100)    NULL,
    StartDate       DATE             NOT NULL,
    EndDate         DATE             NOT NULL,
    ActivityType    NVARCHAR(50)     NOT NULL DEFAULT 'assistencia',
    RequiredHours   FLOAT            NOT NULL DEFAULT 0,
    Notes           NVARCHAR(1000)   NULL,
    CONSTRAINT FK_Schedules_Group     FOREIGN KEY (GroupId)    REFERENCES StudentGroups(Id)    ON DELETE CASCADE,
    CONSTRAINT FK_Schedules_Location  FOREIGN KEY (LocationId) REFERENCES Locations(Id)        ON DELETE CASCADE,
    CONSTRAINT FK_Schedules_Preceptor FOREIGN KEY (PreceptorId) REFERENCES ApplicationUsers(Id) ON DELETE SET NULL
);

CREATE TABLE AttendanceRecords (
    Id                      UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID() PRIMARY KEY,
    StudentId               UNIQUEIDENTIFIER NOT NULL,
    ScheduleId              UNIQUEIDENTIFIER NULL,
    LocationId              UNIQUEIDENTIFIER NOT NULL,
    Type                    NVARCHAR(20)     NOT NULL,
    RecordedAt              DATETIMEOFFSET   NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    Lat                     FLOAT            NOT NULL,
    Lon                     FLOAT            NOT NULL,
    DistanceMeters          FLOAT            NOT NULL DEFAULT 0,
    PhotoUrl                NVARCHAR(1000)   NULL,
    ActivitiesDescription   NVARCHAR(2000)   NULL,
    Status                  NVARCHAR(30)     NOT NULL DEFAULT 'pendente',
    IrregularityReason      NVARCHAR(500)    NULL,
    ValidatedById           UNIQUEIDENTIFIER NULL,
    ValidatedAt             DATETIMEOFFSET   NULL,
    CONSTRAINT FK_Attendance_Student    FOREIGN KEY (StudentId)     REFERENCES ApplicationUsers(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Attendance_Schedule   FOREIGN KEY (ScheduleId)    REFERENCES RotationSchedules(Id) ON DELETE SET NULL,
    CONSTRAINT FK_Attendance_Location   FOREIGN KEY (LocationId)    REFERENCES Locations(Id)        ON DELETE NO ACTION,
    CONSTRAINT FK_Attendance_Validator  FOREIGN KEY (ValidatedById) REFERENCES ApplicationUsers(Id) ON DELETE NO ACTION
);

CREATE TABLE Evaluations (
    Id                  UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID() PRIMARY KEY,
    StudentId           UNIQUEIDENTIFIER NOT NULL,
    PreceptorId         UNIQUEIDENTIFIER NOT NULL,
    ScheduleId          UNIQUEIDENTIFIER NULL,
    ActivitiesScore     INT              NOT NULL,
    PostureScore        INT              NOT NULL,
    PlanningScore       INT              NOT NULL,
    Comment             NVARCHAR(2000)   NULL,
    CreatedAt           DATETIMEOFFSET   NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT FK_Evaluations_Student   FOREIGN KEY (StudentId)   REFERENCES ApplicationUsers(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_Evaluations_Preceptor FOREIGN KEY (PreceptorId) REFERENCES ApplicationUsers(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_Evaluations_Schedule  FOREIGN KEY (ScheduleId)  REFERENCES RotationSchedules(Id) ON DELETE SET NULL
);

CREATE TABLE FormativeFollowups (
    Id                          UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID() PRIMARY KEY,
    StudentId                   UNIQUEIDENTIFIER NOT NULL,
    PreceptorId                 UNIQUEIDENTIFIER NOT NULL,
    ScheduleId                  UNIQUEIDENTIFIER NULL,
    Periodo                     NVARCHAR(100)    NULL,
    -- Postura
    PosturaPontualidade         INT              NOT NULL DEFAULT 3,
    PosturaEtica                INT              NOT NULL DEFAULT 3,
    PosturaResponsabilidade     INT              NOT NULL DEFAULT 3,
    -- Comunicação
    ComunicacaoEquipe           INT              NOT NULL DEFAULT 3,
    ComunicacaoPaciente         INT              NOT NULL DEFAULT 3,
    ComunicacaoEscuta           INT              NOT NULL DEFAULT 3,
    -- Organização
    OrganizacaoPlanejamento     INT              NOT NULL DEFAULT 3,
    OrganizacaoSeguranca        INT              NOT NULL DEFAULT 3,
    OrganizacaoRegistros        INT              NOT NULL DEFAULT 3,
    -- Participação
    ParticipacaoIniciativa      INT              NOT NULL DEFAULT 3,
    ParticipacaoAprendizado     INT              NOT NULL DEFAULT 3,
    ParticipacaoAutocritica     INT              NOT NULL DEFAULT 3,
    -- Texto
    OverallComment              NVARCHAR(3000)   NULL,
    GoalsNextPeriod             NVARCHAR(2000)   NULL,
    StrengthsObserved           NVARCHAR(2000)   NULL,
    AreasForImprovement         NVARCHAR(2000)   NULL,
    PreceptorNotes              NVARCHAR(2000)   NULL,
    -- Status e assinaturas
    Status                      NVARCHAR(50)     NOT NULL DEFAULT 'rascunho',
    PreceptorSignedAt           DATETIMEOFFSET   NULL,
    PreceptorSignerName         NVARCHAR(200)    NULL,
    PreceptorSignerIp           NVARCHAR(100)    NULL,
    PreceptorSignerUserId       UNIQUEIDENTIFIER NULL,
    StudentSignedAt             DATETIMEOFFSET   NULL,
    StudentSignerName           NVARCHAR(200)    NULL,
    StudentSignerIp             NVARCHAR(100)    NULL,
    StudentSignerUserId         UNIQUEIDENTIFIER NULL,
    CreatedAt                   DATETIMEOFFSET   NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    UpdatedAt                   DATETIMEOFFSET   NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT FK_Followups_Student   FOREIGN KEY (StudentId)   REFERENCES ApplicationUsers(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_Followups_Preceptor FOREIGN KEY (PreceptorId) REFERENCES ApplicationUsers(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_Followups_Schedule  FOREIGN KEY (ScheduleId)  REFERENCES RotationSchedules(Id) ON DELETE SET NULL
);
