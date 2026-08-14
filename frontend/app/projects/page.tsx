import Link from "next/link";

import { ProjectCard } from "@/components/cards/ProjectCard";
import { fetchProjects } from "@/lib/api/projects";
import { getAccessToken } from "@/lib/auth";
import { toCardProject } from "@/lib/mappers";

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
