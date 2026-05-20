# Sistema de Sincronização Automática — EstagioCheck

Guia completo para configurar, operar e solucionar problemas da sincronização automática entre os repositórios do projeto EstagioCheck usando GitHub Actions.

---

## Sumário

1. [Visão Geral da Arquitetura](#1-visão-geral-da-arquitetura)
2. [Pré-requisitos](#2-pré-requisitos)
3. [Passo a Passo: Criando o PAT Token](#3-passo-a-passo-criando-o-pat-token)
4. [Passo a Passo: Configurando os Secrets](#4-passo-a-passo-configurando-os-secrets)
5. [Passo a Passo: Criando o Repositório Consolidado](#5-passo-a-passo-criando-o-repositório-consolidado)
6. [Passo a Passo: Configurando o Backend](#6-passo-a-passo-configurando-o-backend)
7. [Passo a Passo: Configurando o Frontend](#7-passo-a-passo-configurando-o-frontend)
8. [Estrutura Final dos Repositórios](#8-estrutura-final-dos-repositórios)
9. [Como Evitar Loop Infinito de Commits](#9-como-evitar-loop-infinito-de-commits)
10. [Como Funciona o rsync (e por que é seguro)](#10-como-funciona-o-rsync-e-por-que-é-seguro)
11. [Arquivos Ignorados na Sincronização](#11-arquivos-ignorados-na-sincronização)
12. [Branches Utilizadas](#12-branches-utilizadas)
13. [Permissões Necessárias](#13-permissões-necessárias)
14. [Erros Comuns e Soluções](#14-erros-comuns-e-soluções)
15. [Como Testar](#15-como-testar)
16. [Comandos Git de Referência](#16-comandos-git-de-referência)
17. [Melhorias Recomendadas](#17-melhorias-recomendadas)

---

## 1. Visão Geral da Arquitetura

```
┌─────────────────────────────────────────────────────────────────┐
│                         FLUXO DE DADOS                          │
│                                                                  │
│  [Backend Repo]  ──push main──▶  [GitHub Actions]  ──▶  ┐      │
│                                                           ├──▶  [Consolidated Repo]
│  [Frontend Repo] ──push main──▶  [GitHub Actions]  ──▶  ┘      │
│                                                                  │
│  • Backend  → /backend  (rsync com --delete)                    │
│  • Frontend → /frontend (rsync com --delete)                    │
│  • Consolidado: somente leitura, sem deploy                     │
└─────────────────────────────────────────────────────────────────┘
```

### Repositórios envolvidos

| Repositório        | Papel                             | Tem Workflow? |
|--------------------|-----------------------------------|---------------|
| `EstagioCheckAPI`  | Fonte do backend                  | ✅ Sim         |
| `<FRONTEND_REPO>`  | Fonte do frontend                 | ✅ Sim         |
| `<CONSOLIDATED>`   | Espelho unificado (somente leitura) | ❌ Não       |

---

## 2. Pré-requisitos

- Conta no GitHub com acesso aos 3 repositórios
- Os 3 repositórios devem estar criados **antes** de configurar os workflows
- Git instalado localmente (apenas para os comandos iniciais)
- Windows como ambiente local (os workflows rodam em `ubuntu-latest` no GitHub)

---

## 3. Passo a Passo: Criando o PAT Token

O **PAT (Personal Access Token)** é o que permite ao GitHub Actions escrever no repositório consolidado em seu nome.

### 3.1 Criar o Token

1. Acesse: https://github.com/settings/tokens
2. Clique em **"Tokens (classic)"** → **"Generate new token (classic)"**
3. Configure:
   - **Note:** `sync-to-consolidated`
   - **Expiration:** `No expiration` (ou 1 ano — anote para renovar)
   - **Scopes:** marque apenas `repo` (Full control of private repositories)
4. Clique em **"Generate token"**
5. **COPIE o token imediatamente** — ele não será exibido novamente

> ⚠️ **Segurança:** Nunca commite o token em nenhum arquivo. Ele fica APENAS nos Secrets do GitHub.

---

## 4. Passo a Passo: Configurando os Secrets

O mesmo PAT deve ser adicionado **individualmente** em cada repositório de origem.

### 4.1 No repositório do Backend (`EstagioCheckAPI`)

1. Acesse o repositório no GitHub
2. Vá em **Settings** → **Secrets and variables** → **Actions**
3. Clique em **"New repository secret"**
4. Preencha:
   - **Name:** `PAT_TOKEN`
   - **Secret:** cole o token gerado no passo 3
5. Clique em **"Add secret"**

### 4.2 No repositório do Frontend

Repita exatamente o mesmo processo no repositório do frontend.

> ℹ️ Os dois repositórios usam o **mesmo** PAT_TOKEN, adicionado **separadamente** em cada um.

---

## 5. Passo a Passo: Criando o Repositório Consolidado

### 5.1 Criar o repositório no GitHub

1. Acesse https://github.com/new
2. Configure:
   - **Repository name:** `EstagioCheck-Unified` (ou outro nome de sua preferência)
   - **Visibility:** Public ou Private (conforme sua preferência)
   - **Initialize with README:** ✅ marque esta opção
3. Clique em **"Create repository"**

### 5.2 Estrutura inicial (execute localmente uma única vez)

```powershell
# Clone o repositório recém-criado
git clone https://github.com/SEU_USUARIO/EstagioCheck-Unified.git
cd EstagioCheck-Unified

# Crie as pastas necessárias com um .gitkeep para preservar a estrutura
New-Item -ItemType Directory -Force -Path backend, frontend
New-Item -ItemType File -Path backend/.gitkeep, frontend/.gitkeep

# Copie os arquivos de template (README e .gitignore)
# → conteúdo disponível em docs/consolidated-repo-template/ neste repositório

# Commit inicial
git add -A
git commit -m "chore: initial structure for consolidated repository"
git push origin main
```

> Após o primeiro push dos workflows, os `.gitkeep` serão substituídos pelos arquivos reais.

---

## 6. Passo a Passo: Configurando o Backend

### 6.1 Editar o workflow

O workflow já foi criado em `.github/workflows/sync-to-consolidated.yml`.  
Abra o arquivo e altere as duas variáveis de ambiente:

```yaml
env:
  CONSOLIDATED_OWNER: "SEU_USUARIO"          # ← seu usuário GitHub
  CONSOLIDATED_REPO:  "EstagioCheck-Unified" # ← nome exato do repo consolidado
```

### 6.2 Commitar e fazer push

```powershell
cd C:\Users\Renan\Desktop\estudos\EstagioCheckApi\EstagioCheckAPI

git add .github/workflows/sync-to-consolidated.yml
git commit -m "ci: add sync workflow to consolidated repository"
git push origin main
```

Após o push, vá em **Actions** no GitHub e acompanhe a execução.

---

## 7. Passo a Passo: Configurando o Frontend

### 7.1 Copiar o workflow template

O template está em `docs/workflows/frontend-sync-to-consolidated.yml` neste repositório.

```powershell
# No repositório do frontend
cd C:\caminho\para\seu\frontend

# Crie a pasta de workflows
New-Item -ItemType Directory -Force -Path .github\workflows

# Copie o arquivo template e renomeie
Copy-Item "C:\...\EstagioCheckAPI\docs\workflows\frontend-sync-to-consolidated.yml" `
          ".github\workflows\sync-to-consolidated.yml"
```

### 7.2 Editar o workflow

Abra `.github/workflows/sync-to-consolidated.yml` no frontend e altere:

```yaml
env:
  CONSOLIDATED_OWNER: "SEU_USUARIO"          # ← seu usuário GitHub
  CONSOLIDATED_REPO:  "EstagioCheck-Unified" # ← nome exato do repo consolidado
```

### 7.3 Commitar e fazer push

```powershell
git add .github/workflows/sync-to-consolidated.yml
git commit -m "ci: add sync workflow to consolidated repository"
git push origin main
```

---

## 8. Estrutura Final dos Repositórios

```
EstagioCheckAPI/                    ← Repositório do Backend (este)
├── .github/
│   └── workflows/
│       └── sync-to-consolidated.yml  ← WORKFLOW DO BACKEND
├── backend/
│   ├── Controllers/
│   ├── Data/
│   ├── DTOs/
│   ├── Migrations/
│   ├── Models/
│   ├── Services/
│   ├── database/
│   ├── Properties/
│   ├── Program.cs
│   ├── EstagioCheck.API.csproj
│   └── appsettings.json
└── docs/
    ├── workflows/
    │   └── frontend-sync-to-consolidated.yml  ← TEMPLATE PARA O FRONTEND
    └── consolidated-repo-template/
        ├── README.md
        └── .gitignore

<FRONTEND_REPO>/                    ← Repositório do Frontend
├── .github/
│   └── workflows/
│       └── sync-to-consolidated.yml  ← WORKFLOW DO FRONTEND (copiado do template)
└── src/
    └── ...

EstagioCheck-Unified/               ← Repositório Consolidado (destino)
├── backend/                        ← sincronizado automaticamente
├── frontend/                       ← sincronizado automaticamente
├── .gitignore
└── README.md
```

---

## 9. Como Evitar Loop Infinito de Commits

Este sistema foi projetado para **não gerar loops** por três razões:

| Mecanismo | Descrição |
|-----------|-----------|
| **Sem workflow no consolidado** | O repo consolidado não tem nenhum workflow. Commits feitos pelo bot não disparam nada. |
| **`paths-ignore: '.github/**'`** | Mudanças no próprio workflow não disparam re-execução. |
| **`concurrency` por grupo** | Evita múltiplas execuções paralelas conflitantes do mesmo workflow. |
| **Verificação de diff** | `git diff --cached --quiet` — só faz push se houver mudanças reais. |

---

## 10. Como Funciona o rsync (e por que é seguro)

O comando usado é:

```bash
rsync -av --delete \
  --exclude='.git/' \
  --exclude='node_modules/' \
  ... \
  ./ _consolidated/backend/
```

| Flag | Comportamento |
|------|---------------|
| `-a` | Modo arquivo: preserva permissões, timestamps, links simbólicos |
| `-v` | Verbose: mostra arquivos transferidos nos logs |
| `--delete` | Remove do destino arquivos que não existem mais na origem |
| `--exclude` | Ignora os padrões listados — **não serão copiados nem apagados** |

> **`--delete` é seguro** porque os `--exclude` protegem pastas como `node_modules/` de serem apagadas no destino mesmo que não existam na origem.

---

## 11. Arquivos Ignorados na Sincronização

### Backend (não sincronizado para o consolidado)

| Padrão | Motivo |
|--------|--------|
| `bin/`, `obj/` | Build artifacts — gerados localmente |
| `appsettings.Development.json` | Credenciais locais |
| `appsettings.Production.json` | Credenciais de produção |
| `.github/` | Workflows não fazem parte do espelho |
| `*.user`, `*.suo` | Arquivos de IDE |

### Frontend (não sincronizado para o consolidado)

| Padrão | Motivo |
|--------|--------|
| `node_modules/` | Dependências — restauradas via `npm install` |
| `.next/`, `dist/`, `build/` | Build artifacts |
| `.env`, `.env.local`, `.env.*` | Variáveis de ambiente sensíveis |
| `coverage/` | Relatórios de teste |

---

## 12. Branches Utilizadas

| Repositório | Branch de origem | Branch de destino |
|-------------|-----------------|-------------------|
| Backend     | `main`          | `main` (consolidado) |
| Frontend    | `main`          | `main` (consolidado) |

> Para alterar a branch monitorada, edite `branches: [main]` nos workflows.

---

## 13. Permissões Necessárias

### PAT Token

| Escopo | Necessário para |
|--------|----------------|
| `repo` | Ler repositórios privados de origem e escrever no consolidado |

> Se todos os repositórios forem **públicos**, o escopo mínimo é `public_repo`.

### Configurações do GitHub

Não é necessário alterar configurações especiais. O GitHub Actions já tem permissão para ler o repositório de origem por padrão. A escrita no consolidado é feita via PAT.

---

## 14. Erros Comuns e Soluções

### ❌ `remote: Permission to ... denied to github-actions[bot]`

**Causa:** O `PAT_TOKEN` não foi adicionado ao repositório de origem, ou o token expirou.

**Solução:**
1. Verifique em Settings → Secrets and variables → Actions se `PAT_TOKEN` existe
2. Gere um novo PAT se o atual expirou
3. Atualize o secret com o novo valor

---

### ❌ `Repository not found`

**Causa:** O nome ou usuário do repositório consolidado está incorreto nas variáveis `env`.

**Solução:** Verifique `CONSOLIDATED_OWNER` e `CONSOLIDATED_REPO` no workflow.

---

### ❌ `error: failed to push some refs — Updates were rejected`

**Causa:** Divergência de histórico entre o clone local e o repositório remoto (commit manual no consolidado).

**Solução:** Nunca faça commits manuais no repositório consolidado. Se acontecer:

```bash
# No repositório consolidado
git fetch origin
git reset --hard origin/main
git push --force   # ⚠️ Apenas no consolidado, jamais nos repos originais
```

---

### ❌ Workflow não dispara após push

**Causa:** O push foi feito em uma branch diferente de `main`, ou a mudança foi apenas em `.github/`.

**Solução:** Verifique se está na branch correta. Mudanças em `.github/**` são intencionalmente ignoradas pelo `paths-ignore`.

---

### ❌ Arquivos da `/frontend` sumiram do consolidado após push no backend

**Causa:** Uso incorreto do rsync apagando arquivos fora do escopo.

**Por que não acontece neste setup:** O rsync opera apenas dentro de `_consolidated/backend/` (ou `_consolidated/frontend/`). Jamais toca na outra pasta.

---

### ❌ `node_modules` ou `bin/obj` foram sincronizados por engano

**Causa:** O `--exclude` não estava presente ou foi removido do workflow.

**Solução:** Restaure os excludes no workflow e faça um push de qualquer arquivo para disparar nova sincronização.

---

## 15. Como Testar

### Teste 1 — Verificar se o workflow é válido (sem push)

```powershell
# Instale o act (executor local de GitHub Actions) — opcional
# https://github.com/nektos/act

# Ou simplesmente valide o YAML online:
# https://rhysd.github.io/actionlint/
```

### Teste 2 — Disparo manual via GitHub UI

1. Acesse o repositório do backend no GitHub
2. Vá em **Actions** → **"🔄 Sync Backend → Consolidated"**
3. Clique em **"Run workflow"** → **"Run workflow"**
4. Acompanhe os logs em tempo real

> Para habilitar disparo manual, adicione `workflow_dispatch:` ao `on:` do workflow.

### Teste 3 — Push real

```powershell
# Faça uma alteração qualquer no backend
echo "# teste sync" >> backend/README_SYNC_TEST.md
git add backend/README_SYNC_TEST.md
git commit -m "test: trigger sync workflow"
git push origin main

# Aguarde ~30 segundos e verifique o repositório consolidado no GitHub
```

### Teste 4 — Verificar exclusão de arquivos

```powershell
# Crie e remova um arquivo para testar o --delete
echo "temp" >> backend/TEMP_FILE.md
git add . ; git commit -m "test: add temp file" ; git push

# Verifique que apareceu no consolidado, então:
git rm backend/TEMP_FILE.md
git commit -m "test: remove temp file (testing rsync --delete)"
git push

# Verifique que sumiu do consolidado também
```

---

## 16. Comandos Git de Referência

### Configuração inicial do repo consolidado (Windows PowerShell)

```powershell
# 1. Clone o repo consolidado recém-criado
git clone https://github.com/SEU_USUARIO/EstagioCheck-Unified.git
cd EstagioCheck-Unified

# 2. Crie a estrutura inicial
New-Item -ItemType Directory -Force -Path backend, frontend
"" | Out-File backend/.gitkeep -Encoding UTF8
"" | Out-File frontend/.gitkeep -Encoding UTF8

# 3. Commit inicial
git add -A
git commit -m "chore: initial folder structure"
git push origin main
```

### Verificar status da sincronização

```powershell
# No repositório consolidado, veja os últimos commits automáticos
git log --oneline -10
```

### Forçar re-sincronização (sem alterar código)

```powershell
# Em qualquer repo de origem, faça um commit vazio
git commit --allow-empty -m "ci: force sync to consolidated"
git push origin main
```

---

## 17. Melhorias Recomendadas

### 17.1 Adicionar disparo manual (recomendado)

Adicione `workflow_dispatch:` para poder disparar manualmente pela UI do GitHub:

```yaml
on:
  push:
    branches: [main]
    paths-ignore: ['.github/**']
  workflow_dispatch:  # ← adicione esta linha
```

### 17.2 Notificação por e-mail em caso de falha

O GitHub já envia e-mail automático quando um workflow falha. Certifique-se que as notificações estão ativas em: Settings → Notifications.

### 17.3 Adicionar badge de status no README principal

```markdown
![Sync Backend](https://github.com/RibeiroRenan04/EstagioCheckAPI/actions/workflows/sync-to-consolidated.yml/badge.svg)
```

### 17.4 Renovação automática do PAT

PATs com expiração requerem renovação manual. Considere usar GitHub Apps (mais robusto) ou adicionar um lembrete no calendário para renovação.

### 17.5 Adicionar `workflow_dispatch` com parâmetros para sincronização forçada

```yaml
workflow_dispatch:
  inputs:
    force_sync:
      description: 'Forçar sincronização mesmo sem mudanças'
      required: false
      default: 'false'
```

### 17.6 Proteger a branch `main` do repositório consolidado

No repo consolidado: Settings → Branches → Add branch protection rule → `main`:
- ✅ Require status checks
- ✅ Restrict who can push → apenas o bot (não bloqueia o Actions)

---

## Checklist de Configuração

Use esta lista para garantir que tudo foi configurado:

- [ ] PAT Token gerado com escopo `repo`
- [ ] Secret `PAT_TOKEN` adicionado no repositório do **backend**
- [ ] Secret `PAT_TOKEN` adicionado no repositório do **frontend**
- [ ] Repositório consolidado criado com estrutura inicial (`backend/` e `frontend/`)
- [ ] Workflow do backend: variáveis `CONSOLIDATED_OWNER` e `CONSOLIDATED_REPO` preenchidas
- [ ] Workflow do backend commitado e pushed para `main`
- [ ] Workflow do frontend copiado para o repo do frontend
- [ ] Workflow do frontend: variáveis preenchidas
- [ ] Workflow do frontend commitado e pushed para `main`
- [ ] Primeiro sync executado com sucesso (verificar na aba Actions)
- [ ] Repositório consolidado verificado com os arquivos corretos

---

*Documentação gerada para o projeto EstagioCheck — Sistema de sincronização via GitHub Actions*
