-- =============================================================================
--  Migration 004 – Consolidar identificador no RGM e remover "Matricula"
--  O RGM é a própria matrícula do aluno. A coluna "Matricula" era apenas um
--  campo redundante (usado só como fallback de exibição do RGM), então foi
--  consolidada no RGM e removida. Executada em produção (Supabase) em 2026-06-14.
-- =============================================================================

-- Migra a matrícula para o RGM apenas em ALUNOS que ainda não têm RGM.
-- (Supervisores tinham matrícula de teste duplicada; foi descartada.)
UPDATE "Usuarios"
SET "Rgm" = "Matricula"
WHERE "Papel" = 'aluno'
  AND ("Rgm" IS NULL OR "Rgm" = '')
  AND "Matricula" IS NOT NULL AND "Matricula" <> '';

ALTER TABLE "Usuarios" DROP COLUMN "Matricula";
