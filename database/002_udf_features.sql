-- =============================================================================
--  Migration 002 – UDF Features
--  Novas colunas em users + tabelas: student_semester_history, password_reset_codes
-- =============================================================================

-- ── Novas colunas em "Users" ──────────────────────────────────────────────────
ALTER TABLE "Users"
    ALTER COLUMN "Email" DROP NOT NULL,
    ADD COLUMN IF NOT EXISTS "Rgm"                VARCHAR(50)      NULL,
    ADD COLUMN IF NOT EXISTS "Semester"           INTEGER          NULL,
    ADD COLUMN IF NOT EXISTS "Shift"              VARCHAR(10)      NULL,
    ADD COLUMN IF NOT EXISTS "Institution"        VARCHAR(200)     NULL,
    ADD COLUMN IF NOT EXISTS "MustChangePassword" BOOLEAN          NOT NULL DEFAULT FALSE,
    ADD COLUMN IF NOT EXISTS "MustSetEmail"       BOOLEAN          NOT NULL DEFAULT FALSE,
    ADD COLUMN IF NOT EXISTS "IsActive"           BOOLEAN          NOT NULL DEFAULT TRUE;

-- Índice único parcial (RGM só quando preenchido)
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Users_Rgm"
    ON "Users" ("Rgm")
    WHERE "Rgm" IS NOT NULL;

-- Ajusta índice único de email para parcial (email pode ser NULL para alunos importados)
DROP INDEX IF EXISTS "IX_Users_Email";
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Users_Email"
    ON "Users" ("Email")
    WHERE "Email" IS NOT NULL;

-- ── Tabela: password_reset_codes ──────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS "PasswordResetCodes" (
    "Id"        SERIAL          PRIMARY KEY,
    "Email"     VARCHAR(255)    NOT NULL,
    "Code"      VARCHAR(6)      NOT NULL,
    "ExpiresAt" TIMESTAMPTZ     NOT NULL,
    "Used"      BOOLEAN         NOT NULL DEFAULT FALSE,
    "CreatedAt" TIMESTAMPTZ     NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS "IX_PasswordResetCodes_Email_Code"
    ON "PasswordResetCodes" ("Email", "Code");

-- ── Tabela: student_semester_history ─────────────────────────────────────────
CREATE TABLE IF NOT EXISTS "StudentSemesterHistories" (
    "Id"         SERIAL          PRIMARY KEY,
    "StudentId"  UUID            NOT NULL REFERENCES "Users"("Id") ON DELETE CASCADE,
    "Semester"   INTEGER         NOT NULL,
    "TotalHours" NUMERIC(10, 2)  NOT NULL DEFAULT 0,
    "RecordedAt" TIMESTAMPTZ     NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS "IX_StudentSemesterHistories_StudentId"
    ON "StudentSemesterHistories" ("StudentId");

-- ── Coluna CodigoCnes em "Locations" ──────────────────────────────────────────
ALTER TABLE "Locations"
    ADD COLUMN IF NOT EXISTS "CodigoCnes" VARCHAR(20) NULL;

CREATE UNIQUE INDEX IF NOT EXISTS "IX_Locations_CodigoCnes"
    ON "Locations" ("CodigoCnes")
    WHERE "CodigoCnes" IS NOT NULL;
