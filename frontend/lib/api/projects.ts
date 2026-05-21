import { apiRequest } from "./request";
import type { Project, ProjectStatus } from "./types";

/**
 * Fetches all projects, optionally filtered by status.
 * @param token - Bearer token from the session
 * @param status - Optional status filter
 * @returns Projects ordered by creation date descending
 */
export function fetchProjects(
  token: string,
  status?: ProjectStatus,
): Promise<Project[]> {
  const query = status ? `?status=${status}` : "";
  return apiRequest<Project[]>(`/api/projects${query}`, token);
}
