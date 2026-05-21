# skill-matrix-llm

A monorepo for the skill-matrix-llm application stack:
**Next.js · C#/.NET API · Keycloak · PostgreSQL · Azure · GitHub Actions**

---

## Repo Layout

```
/
├── frontend/              Next.js 16 App Router (TypeScript, NextAuth + Keycloak)
├── backend/               ASP.NET Core 10 Web API (EF Core + Npgsql, JWT Bearer)
├── infra/                 Bicep modules for Azure provisioning
└── .github/workflows/     GitHub Actions workflows
                           ├── ci-backend.yml        — build & test .NET
                           ├── ci-frontend.yml       — build Next.js
                           ├── cd-infrastructure.yml — deploy Bicep
                           ├── cd-backend.yml        — deploy API (uat→prod)
                           ├── cd-frontend.yml       — deploy frontend (uat→prod)
                           └── cd-orchestrator.yml   — manual orchestrator
```

---

## Prerequisites

| Tool              | Minimum version |
| ----------------- | --------------- |
| Node.js           | 20 LTS          |
| .NET SDK          | 10.0            |
| Azure CLI + Bicep | latest          |
| Keycloak          | 24+             |
| PostgreSQL        | 16+ (local dev) |

---

## Architecture

```
                    ┌─────────────────┐
   Browser ────────▶│  Next.js (App   │
                    │  Router)        │
                    │  frontend/      │◀──── Keycloak (OIDC)
                    └────────┬────────┘           ▲
                             │ Bearer JWT          │
                             ▼                     │
                    ┌─────────────────┐            │
                    │  ASP.NET Core   │────────────┘
                    │  Web API        │  validates JWT
                    │  backend/       │
                    └────────┬────────┘
                             │
                    ┌────────▼────────┐
                    │  PostgreSQL     │
                    │  (EF Core /     │
                    │  Npgsql)        │
                    └─────────────────┘

Azure resources:
  App Service Plan · Frontend App Service · Backend App Service
  Key Vault (RBAC) · Log Analytics Workspace · App Insights
  PostgreSQL Flexible Server · Storage Account · VNet (optional)
```

**frontend/** — Next.js App Router with NextAuth.js handling the Keycloak Authorization Code + PKCE flow. The session stores the Keycloak access token, which is forwarded as a Bearer JWT to the backend.

**backend/** — ASP.NET Core Web API with JWT Bearer middleware that validates Keycloak tokens. Uses EF Core + Npgsql for PostgreSQL access and pulls secrets from Azure Key Vault via Managed Identity.

**infra/** — Bicep modules split into `components/` (create resources), `config/` (configure App Service settings), and `utils/` (shared types and helpers).

**.github/workflows/** — GitHub Actions workflows for CI (build + test) and CD (infrastructure + app deployment) across two cloud environments: uat and prod.

### Azure Resource Naming Convention

All resource names are derived from `appBaseName` and `environment` in each `.bicepparam` file:

| Resource                | Pattern                                | Example (`appBaseName=myapp`, `environment=uat`) |
| ----------------------- | -------------------------------------- | ------------------------------------------------ |
| App Service Plan        | `{appBaseName}-{environment}-asp`      | `myapp-uat-asp`                                  |
| Frontend App Service    | `{appBaseName}-{environment}-frontend` | `myapp-uat-frontend`                             |
| Backend App Service     | `{appBaseName}-{environment}-api`      | `myapp-uat-api`                                  |
| Key Vault               | `{appBaseName}-{environment}-kv`       | `myapp-uat-kv`                                   |
| Log Analytics Workspace | `{appBaseName}-shared-law`             | `myapp-shared-law`                               |
| App Insights            | `{appBaseName}-{environment}-ai`       | `myapp-uat-ai`                                   |
| PostgreSQL Server       | `{appBaseName}-{environment}-postgres` | `myapp-uat-postgres`                             |
| Storage Account         | `{appBaseName}{environment}storage`    | `myappuatstorage`                                |
| VNet                    | `{appBaseName}-{environment}-vnet`     | `myapp-uat-vnet`                                 |

---

## Local Development

Development runs entirely on local machines. Each developer runs the frontend and backend locally against a local PostgreSQL instance and a shared (or local) Keycloak instance.

**Frontend**

```bash
cd frontend
cp .env.example .env.local   # fill in values
npm install
npm run dev
```

**Backend**

```bash
cd backend
# Ensure a local PostgreSQL instance is running
dotnet restore
dotnet run --project src/SkillMatrixLlm.Api
```

---

## Deployment

### 1 — Configure Keycloak

See [docs/keycloak_setup.md](docs/keycloak_setup.md) for the full realm, client, role, and group configuration.

### 2 — Provision Azure Infrastructure

Resource groups are provisioned by IT.

```bash
# Validate the Bicep template
az deployment group validate \
  --resource-group skill-matrix-llm-uat-rg \
  --template-file infra/main.bicep \
  --parameters infra/main.uat.bicepparam \
  --parameters postgresAdminPassword=<password>

# Deploy
az deployment group create \
  --resource-group skill-matrix-llm-uat-rg \
  --template-file infra/main.bicep \
  --parameters infra/main.uat.bicepparam \
  --parameters postgresAdminPassword=<password>
```

- [ ] Confirm resource groups exist for uat and prod
- [ ] Run validate + deploy for uat first, then prod
- [ ] Store secrets in Key Vault: `nextauth-secret`, `keycloak-frontend-client-secret`, `postgres-connection-string`
- [ ] Verify App Service Managed Identities have the `Key Vault Secrets User` role on the vault

### 3 — Set up GitHub Actions

- [ ] Create two GitHub Environments in the repo settings:
  - [ ] `skill-matrix-llm-uat` (1 required reviewer)
  - [ ] `skill-matrix-llm-prod` (2 required reviewers)
- [ ] Add environment secrets to each environment:
  - [ ] `POSTGRES_ADMIN_PASSWORD`
  - [ ] `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID` (for OIDC login)

### 4 — First Deployment

- [ ] Push a branch to trigger `ci-backend.yml` and `ci-frontend.yml` and verify they pass
- [ ] Run `cd-orchestrator.yml` manually targeting `uat` to provision infrastructure and deploy
- [ ] Run EF Core migrations against the uat PostgreSQL instance:
  ```bash
  dotnet ef database update --project backend/src/SkillMatrixLlm.Api
  ```
- [ ] Merge to `main` to trigger the CD workflows for uat deployment
- [ ] Verify end-to-end: frontend → Keycloak login → JWT forwarded to backend → 200 OK

### 5 — Smoke Testing

- [ ] `GET https://{backend-uat-url}/api/health` → 200 (unauthenticated)
- [ ] `GET https://{backend-uat-url}/api/health/auth` with valid JWT → 200
- [ ] Frontend sign-in flow completes without errors
- [ ] Application Insights receives telemetry from both frontend and backend

---

## Frontend — Tailwind Theme

The frontend uses **Tailwind CSS v3** with a custom theme that exposes the full University of Nottingham brand palette as utility classes. Unused classes are excluded from production builds automatically via the `content` glob in `frontend/tailwind.config.ts`.

### Colour utilities

| Class prefix | Example | Hex |
| --- | --- | --- |
| `nottingham-blue` | `bg-nottingham-blue` | `#10263B` |
| `nottingham-blue-{80\|60\|40\|20\|5}` | `text-nottingham-blue-60` | tints |
| `forest-green` / `forest-green-20` | `text-forest-green` | `#005F36` |
| `rebels-gold` / `rebels-gold-20` | `bg-rebels-gold-20` | `#DEB406` |
| `jubilee-red` / `jubilee-red-20` | `border-jubilee-red` | `#B91C2E` |
| `mandarin-orange` / `mandarin-orange-20` | — | `#F98109` |
| `pioneering-pink` / `pioneering-pink-20` | — | `#D7336C` |
| `civic-purple` / `civic-purple-20` | — | `#792D85` |
| `bramley-apple` / `bramley-apple-20` | — | `#93D500` |
| `trent-turquoise` / `trent-turquoise-20` | — | `#37B4B0` |
| `malaysia-blue` / `malaysia-blue-20` | — | `#009BC1` |
| `portland-stone` | `bg-portland-stone` | `#FAF6EF` |
| `paper` | `bg-paper` | `#FDFBF9` |
| `ink` / `ink-{soft\|muted\|faint}` | `text-ink-muted` | Nottingham Blue tints |

**Brand rules** (from the UoN guide — non-negotiable):
- `nottingham-blue` must be visually dominant on every asset.
- Use a maximum of **two** supporting colours per design.
- Never use `black` — use `ink` (`#10263B`) or its tints.
- Never use `white` as a background — use `paper` or `nottingham-blue-5`.

### Typography utilities

Fonts are loaded via `next/font` in `app/layout.tsx` and injected as CSS variables (`--font-geist`, `--font-geist-mono`). The Tailwind `font-sans`, `font-mono`, and `font-display` utilities reference these variables.

The type scale (`text-xs` through `text-5xl`) is overridden to match the UoN design tokens: 12 / 13 / 16 / 17 / 20 / 24 / 32 / 44 / 60 / 84 px.

### Border-radius utilities

| Class | Value |
| --- | --- |
| `rounded-xs` | 2px (tags) |
| `rounded-sm` | 4px (buttons, inputs) |
| `rounded-md` | 6px (cards) |
| `rounded-lg` | 8px (modals) |
| `rounded-pill` | 9999px (filter chips) |

### Shadow utilities

| Class | Use |
| --- | --- |
| `shadow-popover` | Dropdowns, tooltips |
| `shadow-modal` | Dialogs, sheets |

### Breakpoints

| Name | Value | Notes |
| --- | --- | --- |
| `sm` | 640px | Title dampening |
| `md` | 900px | Main reflow — two-column stacks, left rail becomes drawer |
| `lg` | 1200px | Max content width |

---

## Key Design Decisions

### RBAC on Key Vault (not Access Policies)

The Key Vault uses `enableRbacAuthorization: true`. App Service Managed Identities are granted the `Key Vault Secrets User` role via `infra/modules/config/keyvault-access.bicep`. This is the current Microsoft recommendation and is auditable via Azure Policy.

### Sensitive config via Key Vault references

Connection strings and client secrets are stored as Key Vault secrets and referenced in App Service settings as `@Microsoft.KeyVault(SecretUri=...)`. The plain-text values never appear in app settings or source control.

### `postgresAdminPassword` never in source

The PostgreSQL admin password is a `@secure()` Bicep parameter supplied only at deployment time via a GitHub Actions environment secret. It does not appear in any `.bicepparam` file.

### Next.js `output: 'standalone'`

Required for deployment to Azure App Service on Linux. The deployment pipeline copies the `public/` and `.next/static/` directories into the standalone output before packaging, as Next.js does not do this automatically.

### Progressive deployment (uat → prod)

The CD workflows use GitHub Environments with required reviewers as approval gates. Approval policy lives in the GitHub repo settings — not in workflow YAML — keeping pipeline code clean.

### Single `appBaseName` seeds all resource names

Every Azure resource name is derived from `appBaseName` + `environment` variables defined in `main.bicep`. This ensures consistent naming and means renaming the project only requires changing the parameter files.
