/** Project lead contact displayed in ProjectCard footers. */
export interface ProjectLead {
  id?: string;
  /** 1–2 character initials for the Avatar. */
  initials: string;
  name: string;
  /** Avatar gradient index (0–3); defaults to `0` when absent. */
  avatarHue?: 0 | 1 | 2 | 3;
}

/** Research project — mirrors the PROJECTS shape in `data.jsx`. */
export interface Project {
  id?: string;
  title: string;
  /** Primary research discipline label. */
  discipline: string;
  status: string;
  /** Human-readable relative date (e.g. "4 days ago"). */
  posted: string;
  /** When `true` the eyebrow renders in Jubilee Red with "Closing today". */
  deadlineUrgent?: boolean;
  /** Human-readable duration string (e.g. "12 months"). */
  duration: string;
  funded: boolean;
  lead: ProjectLead;
  description: string;
  requiredSkills: string[];
  matchPct: number;
}

/** Staff member profile — mirrors the RESEARCHERS shape in `data.jsx`. */
export interface Profile {
  id?: string;
  /** 1–2 character initials for the Avatar. */
  initials: string;
  name: string;
  /** Job title (e.g. "Associate Professor"). */
  title: string;
  discipline: string;
  /** Short biography paragraph. */
  bio: string;
  skills: string[];
  publications: number;
  hIndex: number;
  openCollaborations: number;
  /** Avatar gradient index (0–3). */
  avatarHue: 0 | 1 | 2 | 3;
  matchPct: number;
  /** Skill labels used to summarise why this person was matched. */
  matchReasons: string[];
}
