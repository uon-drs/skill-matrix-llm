"use server";

import { redirect } from "next/navigation";

import { apiRequest } from "@/lib/api/request";
import type { Project } from "@/lib/api/types";
import { getAccessToken } from "@/lib/auth";

/**
 * Creates a new project and redirects to the projects list on success.
 * @param title - Short title for the project (max 256 chars)
 * @param description - Full description of goals and requirements
 * @param desiredTeamSize - Target number of team members (min 1)
 * @param timeline - Expected timeline or duration
 */
export async function createProject(
  title: string,
  description: string,
  desiredTeamSize: number,
  timeline: string,
): Promise<void> {
  const token = await getAccessToken();
  if (!token) throw new Error("Not authenticated");

  await apiRequest<Project>("/api/projects", token, {
    method: "POST",
    body: JSON.stringify({ title, description, desiredTeamSize, timeline }),
  });

  redirect("/projects");
}
