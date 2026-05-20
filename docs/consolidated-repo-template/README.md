# EstagioCheck — Repositório Consolidado

> **Este repositório é somente leitura.**  
> O conteúdo é sincronizado automaticamente via GitHub Actions a partir dos repositórios originais.  
> **Não faça commits manuais aqui.**

---

## Estrutura

```
EstagioCheck-Unified/
├── backend/        ← sincronizado de RibeiroRenan04/EstagioCheckAPI
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
│
├── frontend/       ← sincronizado de RibeiroRenan04/<FRONTEND_REPO>
│   ├── src/
│   ├── public/
│   └── package.json
│
└── README.md
```

---

## Repositórios Originais

| Componente | Repositório                                      | Branch |
|------------|--------------------------------------------------|--------|
| Backend    | `RibeiroRenan04/EstagioCheckAPI`                 | `main` |
| Frontend   | `RibeiroRenan04/<EstagioCheckFront>`                 | `main` |

---

## Como funciona a sincronização

Cada push no branch `main` dos repositórios originais dispara um GitHub Actions workflow que:

1. Faz checkout do repositório de origem
2. Clona este repositório consolidado
3. Executa `rsync --delete` copiando apenas os arquivos relevantes
4. Commit e push automático (somente se houver diferenças)

---

*Última atualização automática: via GitHub Actions*
