export type SkillLevel = "Basic" | "Intermediate" | "Pro";

export interface UserSkill {
  skillId: string;
  skillName: string;
  level: SkillLevel;
}

export interface UserProfile {
  id: string;
  displayName: string;
  email: string;
  role: string;
  skills: UserSkill[];
}

export interface SkillCatalogueItem {
  id: string;
  name: string;
}
