# Keycloak Realm Setup

This document describes the roles, groups, and claims you need to configure in your Keycloak realm to run this application.

---

## Realm

Name your realm `skill-matrix-llm` (must match `AUTH_KEYCLOAK_ISSUER` and the backend `Realm` config value).

---

## Clients

You need three clients:

---

### `skill-matrix-llm-frontend`

Confidential client used by the Next.js backend (NextAuth).

| Setting | Value |
|---------|-------|
| Client authentication | **On** |
| Authentication flow | Standard flow only |
| Valid redirect URIs | `<frontend-url>/*` e.g. `http://localhost:3000/*` |
| Web origins | `<frontend-url>` e.g. `http://localhost:3000` |

Add an **Audience** mapper so that access tokens include `skill-matrix-llm-api` in the `aud` claim. The backend validates this on every request — without it you get an audience validation error.

- Clients → `skill-matrix-llm-frontend` → Client scopes → `skill-matrix-llm-frontend-dedicated` → Add mapper → **Audience**
- Included client audience: `skill-matrix-llm-api` — Add to access token: **On**

Copy the client secret from the **Credentials** tab into `AUTH_KEYCLOAK_SECRET` (`.env.local` locally, Key Vault in Azure).

---

### `skill-matrix-llm-api`

Confidential client representing the backend API — used as the JWT audience and for Admin API machine-to-machine calls.

| Setting | Value |
|---------|-------|
| Client authentication | **On** |
| Authentication flow | Standard flow only |

Copy the client secret into `Keycloak__Secret` in App Service config (or Key Vault reference).

---

### `skill-matrix-llm-public`

Public client for developer API docs via Scalar — **local development only**, do not create in production.

| Setting | Value |
|---------|-------|
| Client authentication | **Off** (public client) |
| Authentication flow | Standard flow only |
| Valid redirect URIs | `http://localhost:5000/*` |
| Web origins | `http://localhost:5000` |

No secret required. PKCE (SHA-256) is enforced by the Scalar configuration.

---

After creating the clients, update `AUTH_KEYCLOAK_ISSUER` in `frontend/.env.example` and App Service config, and update the Keycloak URLs in the `infra/main.*.bicepparam` files.

---

## Roles

The seeder creates these as **realm roles**, which is fine — this is the only app on this realm so there is no namespace collision concern. The claims transformer reads from both `realm_access.roles` and `resource_access.{clientId}.roles` in the JWT, so either approach works.

### Roles to create

| Role name | What it grants |
|-----------|----------------|
| `CreateUsers` | Create new users in Keycloak via the API |
| `UpdateUsers` | Update user details and assign roles to other users |
| `DeleteUsers` | Delete users |
| `ViewUsers` | Read the user list and individual user profiles |
| `ManageSkills` | Create, rename, and delete skills in the catalogue |
| `ManageProjects` | Create and manage projects; trigger LLM recommendations |

> **`SendHealthCheckEmails`** and **`ViewContent`** are defined in code but not enforced as distinct policies — the health check endpoint actually requires `ViewUsers`, and `ViewContent` is currently unused. You do not need to create these in Keycloak unless you plan to use them in future.

---

## Groups

Create the following groups in the realm:

| Group name | Roles assigned | Intended members |
|------------|---------------|-----------------|
| `Admin` | All roles | Administrators |
| `Users` | `ViewContent`, `ManageProjects`, `ManageSkills` | Regular authenticated users |
| `Guest` | `ViewContent` | Read-only viewers |

Assign the realm roles to each group using **Group Role Mappings** rather than assigning roles directly to individual users. This makes access management much easier.

### Default group for self-registration

The app uses Keycloak's own registration UI (via `prompt=create`), so new users who sign up themselves are not created through the backend — no code runs to assign them a group. Without a default group they would land in the app authenticated but with no roles, and every policy-gated endpoint would return 403.

Set the default group to `Users` in **Realm Settings → General → Default Groups**. Every self-registered user is then automatically placed in `Users` after verifying their email, which is the right level of access for anyone signing up.

> Email verification is already enabled, which is sufficient access control for this app — invite-only was considered but rejected because anyone who would be invited would get standard `Users` access anyway, so the extra admin overhead buys nothing.

> **Ownership enforcement — current state:**
>
> - **User skills:** enforced. `UsersController` runs an `IsSelfOrAdmin` check before add/update/remove skill endpoints — the request is allowed only if the caller holds the `UpdateUsers` Keycloak role (admin) or their `sub` claim resolves to the same user as the path parameter.
> - **Projects:** not yet enforced. `Update`, `TransitionStatus`, `Close`, and all team management endpoints (`CreateTeam`, `AddTeamMember`, `RemoveTeamMember`, `ConfirmTeam`, `RejectTeam`) execute without any caller-vs-owner check. Any user with the `ManageProjects` Keycloak role can currently mutate any project.
>
> Project ownership enforcement is planned but not yet implemented.

---

## Standard claims (no configuration needed)

The following claims are read from the JWT by the backend but are standard OIDC claims that Keycloak includes automatically — you do not need to add custom mappers for them:

| Claim | Source | Used for |
|-------|--------|---------|
| `sub` | Always present | Keycloak user ID, stored as the primary key in the app DB |
| `name` | `profile` scope | Display name shown in the UI |
| `preferred_username` | `profile` scope | Fallback display name if `name` is absent |
| `email` | `email` scope | Shown on profile; used when sending health-check emails |

Ensure the `profile` and `email` scopes are added to the `skill-matrix-llm-frontend` client (they are included by default in new Keycloak clients).

---

## Token mapper for client roles

Keycloak does **not** include `resource_access` in access tokens by default for all client configurations. Verify this is working:

1. In your realm, go to **Clients → skill-matrix-llm-api → Client scopes**.
2. Open the dedicated scope (`skill-matrix-llm-api-dedicated`).
3. Confirm a **"roles"** mapper of type **"User Client Role"** exists and has **"Add to access token"** enabled.

If it is missing, add it manually:
- Mapper type: `User Client Role`
- Client ID: `skill-matrix-llm-api`
- Token claim name: `resource_access.${client_id}.roles`
- Multivalued: on
- Add to access token: on

---

## Summary checklist

- [ ] Realm named `skill-matrix-llm`
- [ ] Client `skill-matrix-llm-frontend` created with redirect URIs and audience mapper
- [ ] Client `skill-matrix-llm-api` created (confidential, client credentials enabled)
- [ ] Client `skill-matrix-llm-public` created (local dev only)
- [ ] Six realm roles created: `CreateUsers`, `UpdateUsers`, `DeleteUsers`, `ViewUsers`, `ManageSkills`, `ManageProjects`
- [ ] Groups `Admin`, `Users`, and `Guest` created with appropriate role mappings
- [ ] Default group set to `Users` (Realm Settings → General → Default Groups)
- [ ] `profile` and `email` scopes on the frontend client
- [ ] `resource_access` included in access tokens (verify mapper above)
