import { apiRequest } from "./request";
import type { SkillCatalogueItem } from "./types";

/**
 * Searches the skill catalogue.
 * @param query - Search string (matches skill name)
 * @param token - Bearer token from the session
 * @returns Matching skills from the catalogue
 */
export function searchSkills(
  query: string,
  token: string,
): Promise<SkillCatalogueItem[]> {
  const params = query ? `?search=${encodeURIComponent(query)}` : "";
  return apiRequest<SkillCatalogueItem[]>(`/api/skills${params}`, token);
}
