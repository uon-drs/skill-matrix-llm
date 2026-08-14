import type { Project as CardProject } from "@/components/cards/types";
import { StatusPill } from "@/components/core/StatusPill";
import type { Project, ProjectStatus } from "@/lib/api/types";

/**
 * Formats an ISO date string as a short relative-time label (e.g. "3 days ago").
 * @param iso - ISO 8601 date string
 * @returns Human-readable relative date
 */
export function toRelativeDate(iso: string): string {
  const diff = Date.now() - new Date(iso).getTime();
  const days = Math.floor(diff / 86_400_000);
  if (days === 0) return "Today";
  if (days === 1) return "Yesterday";
  if (days < 30) return `${days} days ago`;
  const months = Math.floor(days / 30);
  return months === 1 ? "1 month ago" : `${months} months ago`;
}

/**
 * Maps an API project to the shape ProjectCard expects.
 * @param project - Project returned from the API
 * @returns Card-ready project data
 */
export function toCardProject(project: Project): CardProject {
  const initials = project.createdByUser.displayName
    .split(" ")
    .map((w) => w[0])
    .join("")
    .slice(0, 2)
    .toUpperCase();

  return {
    id: project.id,
    title: project.title,
    description: project.description,
    status: project.status,
    posted: toRelativeDate(project.createdAt),
    duration: project.timeline,
    funded: false,
    requiredSkills: [],
    lead: { initials, name: project.createdByUser.displayName },
  };
}

/**
 * Maps a project status to its corresponding StatusPill element.
 * @param projectStatus - Status of the associated project
 * @returns A StatusPill element, or null for statuses with no pill mapping
 */
export function toStatusPill(projectStatus: ProjectStatus) {
  switch (projectStatus) {
    case "Open":
      return <StatusPill status="open" />;
    case "TeamConfirmed":
      return <StatusPill status="accepted" label="Team confirmed" />;
    case "Closed":
      return <StatusPill status="closed" />;
    default:
      return null;
  }
}
