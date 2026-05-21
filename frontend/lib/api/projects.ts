import { apiRequest, apiRequestNoContent } from "./request";
import type { Project, ProjectDetail, ProjectStatus } from "./types";

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

/**
 * Fetches full project detail including teams and recommendations.
 * @param projectId - Project ID
 * @param token - Bearer token from the session
 * @returns Project detail with teams and recommendations
 */
export function fetchProject(
  projectId: string,
  token: string,
): Promise<ProjectDetail> {
  return apiRequest<ProjectDetail>(`/api/projects/${projectId}`, token);
}

/**
 * Dispatches a team recommendation request for the specified project to the LLM service.
 * @param projectId - Project ID
 * @param token - Bearer token from the session
 */
export async function triggerRecommendation(
  projectId: string,
  token: string,
): Promise<void> {
  await apiRequestNoContent(
    `/api/projects/${projectId}/recommendations`,
    token,
    { method: "POST" },
  );
}
