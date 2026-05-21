export type SkillLevel = "Basic" | "Intermediate" | "Pro";

export type ProjectStatus = "Draft" | "Open" | "TeamConfirmed" | "Closed";

export interface UserDto {
  id: string;
  displayName: string;
  email: string;
  role: string;
}

export interface Project {
  id: string;
  title: string;
  description: string;
  desiredTeamSize: number;
  timeline: string;
  status: ProjectStatus;
  createdByUser: UserDto;
  createdAt: string;
}

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
