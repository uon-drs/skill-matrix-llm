import Link from "next/link";
import { notFound, redirect } from "next/navigation";

import { fetchProject } from "@/lib/api/projects";
import { fetchCurrentUserProfile } from "@/lib/api/users";
import { getAccessToken } from "@/lib/auth";

import { ProjectDetails } from "./_ProjectDetails";
import { SubmitForAnalysisButton } from "./_SubmitForAnalysisButton";
import { TeamCard } from "./_TeamCard";

/**
 * Displays full project detail and allows the owner to submit the project for LLM analysis.
 */
export default async function ProjectDetailPage({
  params,
}: {
  params: Promise<{ projectId: string }>;
}) {
  const { projectId } = await params;

  const token = await getAccessToken();
  if (!token) redirect("/");

  let project;
  let currentUser;
  try {
    [project, currentUser] = await Promise.all([
      fetchProject(projectId, token),
      fetchCurrentUserProfile(token),
    ]);
  } catch {
    notFound();
  }

  const canSubmit = project.status === "Draft" || project.status === "Open";
  const canManage = project.createdByUser.id === currentUser.id;

  return (
    <main className="bg-paper px-6 py-10 min-h-[calc(100vh-60px)]">
      <div className="max-w-3xl mx-auto flex flex-col gap-8">
        {/* Back link */}
        <Link
          href="/projects"
          className="text-[13px] text-ink-soft hover:text-ink transition-colors duration-[120ms] w-fit"
        >
          ← Projects
        </Link>

        {/* Title, status, metadata, description — with edit/publish/close controls */}
        <ProjectDetails project={project} canManage={canManage} />

        {/* LLM analysis */}
        <div className="flex flex-col gap-2 border border-[var(--border)] rounded-sm p-5">
          <h2 className="text-[15px] font-semibold text-ink">Team analysis</h2>
          {project.recommendations.length > 0 && (
            <p className="text-[13px] text-ink-soft">
              {project.recommendations.length === 1
                ? "1 analysis has been run for this project."
                : `${project.recommendations.length} analyses have been run for this project.`}
            </p>
          )}
          {canSubmit ? (
            <SubmitForAnalysisButton projectId={project.id} />
          ) : (
            <p className="text-[13px] text-ink-muted">
              Analysis is not available for projects with status{" "}
              <strong>{project.status}</strong>.
            </p>
          )}
        </div>

        {/* Teams */}
        {project.teams.length > 0 && (
          <div className="flex flex-col gap-3">
            <h2 className="text-[15px] font-semibold text-ink">
              Proposed teams
            </h2>
            <div className="flex flex-col gap-3">
              {project.teams.map((team) => (
                <TeamCard
                  key={team.id}
                  projectId={project.id}
                  team={team}
                  canManage={canManage}
                />
              ))}
            </div>
          </div>
        )}
      </div>
    </main>
  );
}
