import Link from "next/link";

import { ProjectCard } from "@/components/cards/ProjectCard";
import { fetchProjects } from "@/lib/api/projects";
import type { Project } from "@/lib/api/types";
import { getAccessToken } from "@/lib/auth";

function toRelativeDate(iso: string): string {
  const diff = Date.now() - new Date(iso).getTime();
  const days = Math.floor(diff / 86_400_000);
  if (days === 0) return "Today";
  if (days === 1) return "Yesterday";
  if (days < 30) return `${days} days ago`;
  const months = Math.floor(days / 30);
  return months === 1 ? "1 month ago" : `${months} months ago`;
}

function toCardProject(project: Project) {
  const nameParts = project.createdByUser.displayName.split(" ");
  const initials = nameParts
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
 * Lists all projects the authenticated user can see.
 */
export default async function ProjectsPage() {
  const token = (await getAccessToken())!;
  const projects = await fetchProjects(token);

  return (
    <main className="bg-paper px-6 py-10 min-h-[calc(100vh-60px)]">
      <div className="max-w-3xl mx-auto flex flex-col gap-6">
        <div className="flex items-center justify-between">
          <h1 className="text-[22px] font-semibold tracking-[-0.02em] text-ink">
            Projects
          </h1>
          <Link
            href="/projects/new"
            className="px-4 py-2 text-[14px] font-medium rounded-sm bg-nottingham-blue text-paper hover:bg-nottingham-blue/90 transition-colors duration-[120ms]"
          >
            New project
          </Link>
        </div>

        {projects.length === 0 ? (
          <p className="text-[14px] text-ink-muted">No projects yet.</p>
        ) : (
          <div className="flex flex-col gap-3">
            {projects.map((project) => (
              <ProjectCard
                key={project.id}
                project={toCardProject(project)}
                href={`/projects/${project.id}`}
              />
            ))}
          </div>
        )}
      </div>
    </main>
  );
}
