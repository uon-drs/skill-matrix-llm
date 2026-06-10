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

export type TeamStatus = "Proposed" | "Confirmed" | "Rejected";
export type MembershipStatus =
  | "Invited"
  | "Accepted"
  | "Declined"
  | "Requested";
export type ProjectSource = "LlmGenerated" | "ManuallyAssembled";

export interface TeamMembership {
  id: string;
  user: UserDto;
  projectRole: string;
  membershipStatus: MembershipStatus;
}

export interface Team {
  id: string;
  source: ProjectSource;
  status: TeamStatus;
  createdAt: string;
  members: TeamMembership[];
}

export interface RecommendationRecord {
  id: string;
  projectId: string;
  teamId: string;
  rawResponse: string;
  createdAt: string;
}

export interface ProjectDetail extends Project {
  teams: Team[];
  recommendations: RecommendationRecord[];
}

export interface UserTeamMembershipDto {
  teamId: string;
  projectId: string;
  projectTitle: string;
  projectStatus: ProjectStatus;
  teamStatus: TeamStatus;
  projectRole: string;
  membershipStatus: MembershipStatus;
}
