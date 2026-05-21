import Link from "next/link";
import { notFound, redirect } from "next/navigation";

import { fetchProject } from "@/lib/api/projects";
import type { ProjectStatus } from "@/lib/api/types";
import { getAccessToken } from "@/lib/auth";
import { cn } from "@/lib/utils";

import { SubmitForAnalysisButton } from "./_SubmitForAnalysisButton";

function toRelativeDate(iso: string): string {
  const diff = Date.now() - new Date(iso).getTime();
  const days = Math.floor(diff / 86_400_000);
  if (days === 0) return "Today";
  if (days === 1) return "Yesterday";
  if (days < 30) return `${days} days ago`;
  const months = Math.floor(days / 30);
  return months === 1 ? "1 month ago" : `${months} months ago`;
}

const STATUS_STYLES: Record<ProjectStatus, string> = {
  Draft: "bg-portland-stone text-ink-soft",
  Open: "bg-nottingham-blue-5 text-nottingham-blue",
  TeamConfirmed: "bg-green-100 text-green-800",
  Closed: "bg-portland-stone text-ink-muted",
};

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
  try {
    project = await fetchProject(projectId, token);
  } catch {
    notFound();
  }

  const canSubmit = project.status === "Draft" || project.status === "Open";

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

        {/* Header */}
        <div className="flex flex-col gap-3">
          <div className="flex items-start gap-3 flex-wrap">
            <h1 className="text-[22px] font-semibold tracking-[-0.02em] text-ink flex-1">
              {project.title}
            </h1>
            <span
              className={cn(
                "px-2 py-0.5 text-[12px] font-medium rounded-pill shrink-0",
                STATUS_STYLES[project.status],
              )}
            >
              {project.status}
            </span>
          </div>

          {/* Metadata row */}
          <div className="flex flex-wrap gap-x-5 gap-y-1 text-[13px] text-ink-soft">
            <span>Team size: {project.desiredTeamSize}</span>
            <span>Timeline: {project.timeline}</span>
            <span>Created by {project.createdByUser.displayName}</span>
            <span>{toRelativeDate(project.createdAt)}</span>
          </div>
        </div>

        {/* Description */}
        <p className="text-[15px] text-ink leading-relaxed whitespace-pre-wrap">
          {project.description}
        </p>

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
                <div
                  key={team.id}
                  className="border border-[var(--border)] rounded-sm p-4 flex flex-col gap-2"
                >
                  <div className="flex items-center justify-between">
                    <span className="text-[13px] font-medium text-ink">
                      Team
                    </span>
                    <span className="text-[12px] text-ink-soft">
                      {team.status}
                    </span>
                  </div>
                  {team.members.length > 0 ? (
                    <ul className="flex flex-col gap-1">
                      {team.members.map((member) => (
                        <li
                          key={member.id}
                          className="text-[13px] text-ink-soft"
                        >
                          {member.user.displayName}
                          {member.projectRole && (
                            <span className="text-ink-muted">
                              {" "}
                              — {member.projectRole}
                            </span>
                          )}
                        </li>
                      ))}
                    </ul>
                  ) : (
                    <p className="text-[13px] text-ink-muted">
                      No members yet.
                    </p>
                  )}
                </div>
              ))}
            </div>
          </div>
        )}
      </div>
    </main>
  );
}
