import { apiRequest } from "./request";
import type {
  TeamMembership,
  UserProfile,
  UserTeamMembershipDto,
} from "./types";

/**
 * Syncs the current user's Keycloak identity into the local DB (creating the
 * record on first login) and returns their full profile. Safe to call on every
 * page load — the upsert is idempotent.
 * @param token - Bearer token from the session
 * @returns The current user's profile including skills
 */
export function syncAndFetchCurrentUser(token: string): Promise<UserProfile> {
  return apiRequest<UserProfile>("/api/users/login", token, { method: "POST" });
}

/**
 * Fetches the current authenticated user's profile (requires prior login sync).
 * @param token - Bearer token from the session
 * @returns The current user's profile including skills
 */
export function fetchCurrentUserProfile(token: string): Promise<UserProfile> {
  return apiRequest<UserProfile>("/api/users/me", token);
}

/**
 * Fetches any user's profile by their application database ID.
 * @param userId - Application user GUID
 * @param token - Bearer token from the session
 * @returns The user's profile including skills
 */
export function fetchUserProfile(
  userId: string,
  token: string,
): Promise<UserProfile> {
  return apiRequest<UserProfile>(`/api/users/${userId}`, token);
}

/**
 * Fetches all team memberships for the current authenticated user.
 * @param token - Bearer token from the session
 * @returns All memberships with project and team context
 */
export function fetchMyMemberships(
  token: string,
): Promise<UserTeamMembershipDto[]> {
  return apiRequest<UserTeamMembershipDto[]>("/api/users/me/teams", token);
}

/**
 * Accepts a team invitation or self-request.
 * @param teamId - Team GUID
 * @param token - Bearer token from the session
 * @returns The updated membership record
 */
export function acceptMembership(
  teamId: string,
  token: string,
): Promise<TeamMembership> {
  return apiRequest<TeamMembership>(
    `/api/users/me/memberships/${teamId}/accept`,
    token,
    { method: "PUT" },
  );
}

/**
 * Declines a team invitation or self-request.
 * @param teamId - Team GUID
 * @param token - Bearer token from the session
 * @returns The updated membership record
 */
export function declineMembership(
  teamId: string,
  token: string,
): Promise<TeamMembership> {
  return apiRequest<TeamMembership>(
    `/api/users/me/memberships/${teamId}/decline`,
    token,
    { method: "PUT" },
  );
}
