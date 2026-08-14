"use server";

import { revalidatePath } from "next/cache";

import {
  closeProject,
  confirmTeam,
  rejectTeam,
  transitionProjectStatus,
  triggerRecommendation,
  updateProject,
} from "@/lib/api/projects";
import { ApiError } from "@/lib/api/request";
import { getAccessToken } from "@/lib/auth";

/**
 * Submits the project to the LLM analysis queue.
 * @param projectId - Project ID to submit
 * @returns An empty object on success, or `{ error }` on failure
 */
export async function submitForAnalysis(
  projectId: string,
): Promise<{ error?: string }> {
  const token = await getAccessToken();
  if (!token) return { error: "Not authenticated." };

  try {
    await triggerRecommendation(projectId, token);
    return {};
  } catch (err) {
    if (err instanceof ApiError && err.status === 409) {
      return {
        error: "Analysis already queued or project is not in a valid state.",
      };
    }
    return { error: "Failed to submit for analysis. Please try again." };
  }
}

/**
 * Updates a project's details on behalf of the project's creator.
 * @param projectId - Project ID
 * @param request - Updated title, description, desired team size, and timeline
 * @returns An empty object on success, or `{ error }` on failure
 */
export async function updateProjectAction(
  projectId: string,
  request: {
    title: string;
    description: string;
    desiredTeamSize: number;
    timeline: string;
  },
): Promise<{ error?: string }> {
  const token = await getAccessToken();
  if (!token) return { error: "Not authenticated." };

  try {
    await updateProject(projectId, request, token);
    revalidatePath(`/projects/${projectId}`);
    revalidatePath("/projects");
    return {};
  } catch (err) {
    if (err instanceof ApiError && err.status === 403) {
      return { error: "You don't have permission to manage this project." };
    }
    if (err instanceof ApiError && err.status === 409) {
      return { error: "This project can no longer be edited." };
    }
    return { error: "Failed to update project. Please try again." };
  }
}

/**
 * Publishes a project, transitioning it from Draft to Open.
 * @param projectId - Project ID
 * @returns An empty object on success, or `{ error }` on failure
 */
export async function publishProjectAction(
  projectId: string,
): Promise<{ error?: string }> {
  const token = await getAccessToken();
  if (!token) return { error: "Not authenticated." };

  try {
    await transitionProjectStatus(projectId, "Open", token);
    revalidatePath(`/projects/${projectId}`);
    revalidatePath("/projects");
    return {};
  } catch (err) {
    if (err instanceof ApiError && err.status === 403) {
      return { error: "You don't have permission to manage this project." };
    }
    if (err instanceof ApiError && err.status === 409) {
      return {
        error: "This project cannot be published from its current status.",
      };
    }
    return { error: "Failed to publish project. Please try again." };
  }
}

/**
 * Closes a project on behalf of the project's creator.
 * @param projectId - Project ID
 * @returns An empty object on success, or `{ error }` on failure
 */
export async function closeProjectAction(
  projectId: string,
): Promise<{ error?: string }> {
  const token = await getAccessToken();
  if (!token) return { error: "Not authenticated." };

  try {
    await closeProject(projectId, token);
    revalidatePath(`/projects/${projectId}`);
    revalidatePath("/projects");
    return {};
  } catch (err) {
    if (err instanceof ApiError && err.status === 403) {
      return { error: "You don't have permission to manage this project." };
    }
    if (err instanceof ApiError && err.status === 409) {
      return { error: "This project is already closed." };
    }
    return { error: "Failed to close project. Please try again." };
  }
}

/**
 * Confirms a proposed team on behalf of the project's creator.
 * @param projectId - Project ID
 * @param teamId - Team ID to confirm
 * @returns An empty object on success, or `{ error }` on failure
 */
export async function confirmTeamAction(
  projectId: string,
  teamId: string,
): Promise<{ error?: string }> {
  const token = await getAccessToken();
  if (!token) return { error: "Not authenticated." };

  try {
    await confirmTeam(projectId, teamId, token);
    revalidatePath(`/projects/${projectId}`);
    revalidatePath("/projects");
    return {};
  } catch (err) {
    if (err instanceof ApiError && err.status === 403) {
      return { error: "You don't have permission to manage this project." };
    }
    if (err instanceof ApiError && err.status === 409) {
      return { error: "This team is no longer awaiting a decision." };
    }
    return { error: "Failed to confirm team. Please try again." };
  }
}

/**
 * Rejects a proposed team on behalf of the project's creator.
 * @param projectId - Project ID
 * @param teamId - Team ID to reject
 * @returns An empty object on success, or `{ error }` on failure
 */
export async function rejectTeamAction(
  projectId: string,
  teamId: string,
): Promise<{ error?: string }> {
  const token = await getAccessToken();
  if (!token) return { error: "Not authenticated." };

  try {
    await rejectTeam(projectId, teamId, token);
    revalidatePath(`/projects/${projectId}`);
    revalidatePath("/projects");
    return {};
  } catch (err) {
    if (err instanceof ApiError && err.status === 403) {
      return { error: "You don't have permission to manage this project." };
    }
    if (err instanceof ApiError && err.status === 409) {
      return { error: "This team is no longer awaiting a decision." };
    }
    return { error: "Failed to reject team. Please try again." };
  }
}
