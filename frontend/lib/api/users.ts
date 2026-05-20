import { apiRequest } from "./request";
import type { UserProfile } from "./types";

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
