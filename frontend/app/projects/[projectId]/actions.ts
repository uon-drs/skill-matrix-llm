"use server";

import { triggerRecommendation } from "@/lib/api/projects";
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
