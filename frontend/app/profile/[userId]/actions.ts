"use server";

import { revalidatePath } from "next/cache";

import { apiRequest } from "@/lib/api/request";
import type { SkillLevel, UserSkill } from "@/lib/api/types";
import { getAccessToken } from "@/lib/auth";

/**
 * Adds a skill to a user's profile by name, creating the catalogue entry if it
 * doesn't already exist (requires ManageSkills role for new skills).
 * @param userId - Application user ID
 * @param skillName - Display name of the skill
 * @param level - Proficiency level
 */
export async function addSkillByName(
  userId: string,
  skillName: string,
  level: SkillLevel,
): Promise<UserSkill> {
  const token = await getAccessToken();
  if (!token) throw new Error("Not authenticated");

  const trimmed = skillName.trim();
  if (!trimmed) throw new Error("Skill name is required");

  // Reuse an existing catalogue entry with the same name, otherwise create one
  const existing = await apiRequest<{ id: string; name: string }[]>(
    `/api/skills?search=${encodeURIComponent(trimmed)}`,
    token,
  );
  const match = existing.find(
    (s) => s.name.toLowerCase() === trimmed.toLowerCase(),
  );

  let skillId: string;
  if (match) {
    skillId = match.id;
  } else {
    const created = await apiRequest<{ id: string; name: string }>(
      "/api/skills",
      token,
      { method: "POST", body: JSON.stringify({ name: trimmed }) },
    );
    skillId = created.id;
  }

  const result = await apiRequest<UserSkill>(
    `/api/users/${userId}/skills`,
    token,
    { method: "POST", body: JSON.stringify({ skillId, level }) },
  );

  revalidatePath(`/profile/${userId}`);
  return result;
}

/**
 * Updates the proficiency level for an existing skill on a user's profile.
 * @param userId - Application user ID
 * @param skillId - Skill catalogue ID
 * @param level - New proficiency level
 */
export async function updateSkillLevel(
  userId: string,
  skillId: string,
  level: SkillLevel,
): Promise<UserSkill> {
  const token = await getAccessToken();
  if (!token) throw new Error("Not authenticated");

  const result = await apiRequest<UserSkill>(
    `/api/users/${userId}/skills/${skillId}`,
    token,
    {
      method: "PUT",
      body: JSON.stringify({ level }),
    },
  );

  revalidatePath(`/profile/${userId}`);
  return result;
}

/**
 * Removes a skill from a user's profile.
 * @param userId - Application user ID
 * @param skillId - Skill catalogue ID
 */
export async function removeSkill(
  userId: string,
  skillId: string,
): Promise<void> {
  const token = await getAccessToken();
  if (!token) throw new Error("Not authenticated");

  await fetch(
    `${process.env.NEXT_PUBLIC_API_BASE_URL ?? ""}/api/users/${userId}/skills/${skillId}`,
    {
      method: "DELETE",
      headers: { Authorization: `Bearer ${token}` },
    },
  );

  revalidatePath(`/profile/${userId}`);
}
