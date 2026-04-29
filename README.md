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

Create two clients in your Keycloak realm:

**`skill-matrix-llm-frontend`** — confidential client used by the Next.js backend (NextAuth)

| Setting | Value |
| --- | --- |
| Client authentication | **On** |
| Authentication flow | Standard flow only |
| Valid redirect URIs | `<frontend-url>` e.g., `http://localhost:3000/*` |
| Web origins | `<frontend-url>` e.g., `http://localhost:3000` |

Add an **Audience** mapper to the frontend client so that the access tokens it receives include `skill-matrix-llm-api` in the `aud` claim. The backend validates this claim on every request — without it you will get an audience validation error.
  - Clients → `skill-matrix-llm-frontend` → Client scopes → `skill-matrix-llm-frontend-dedicated` → Add mapper → **Audience**
  - Included client audience: `skill-matrix-llm-api` — Add to access token: **On**

Copy the client secret from the **Credentials** tab into `AUTH_KEYCLOAK_SECRET` (`.env` locally, Key Vault in Azure).

---

**`skill-matrix-llm-api`** — confidential client representing the backend API (used as the JWT audience)

| Setting | Value |
| --- | --- |
| Client authentication | **On** |
| Authentication flow | Standard flow only |

Copy the client secret into `Keycloak__Secret` in App Service config (or Key Vault reference).

---

**`skill-matrix-llm-public`** — public client for developer API docs via Scalar — **local development only**

| Setting | Value |
| --- | --- |
| Client authentication | **Off** (public client) |
| Authentication flow | Standard flow only |
| Valid redirect URIs | `http://localhost:5000/*` |
| Web origins | `http://localhost:5000` |

No secret is required. PKCE (SHA-256) is enforced by the Scalar configuration.

---

Update Keycloak URLs in `infra/main.*.bicepparam` files and `AUTH_KEYCLOAK_ISSUER` in frontend `.env.example` and App Service config.

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
