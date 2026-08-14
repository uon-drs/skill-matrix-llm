"use client";

import { useState, useTransition } from "react";

import { Button } from "@/components/core/Button";
import type { ProjectDetail, ProjectStatus } from "@/lib/api/types";
import { toRelativeDate } from "@/lib/mappers";
import { cn } from "@/lib/utils";

import { EditProjectForm } from "./_EditProjectForm";
import { closeProjectAction, publishProjectAction } from "./actions";

const STATUS_STYLES: Record<ProjectStatus, string> = {
  Draft: "bg-portland-stone text-ink-soft",
  Open: "bg-nottingham-blue-5 text-nottingham-blue",
  TeamConfirmed: "bg-green-100 text-green-800",
  Closed: "bg-portland-stone text-ink-muted",
};

interface ProjectDetailsProps {
  project: ProjectDetail;
  /** Whether the current viewer is the project's creator. */
  canManage: boolean;
}

/**
 * Displays the project's title, status, metadata, and description, with
 * edit/publish/close controls for the project's creator. Toggles to an
 * inline edit form when Edit is clicked.
 * @param project - The project to display
 * @param canManage - Whether the current viewer may edit/publish/close this project
 */
export function ProjectDetails({ project, canManage }: ProjectDetailsProps) {
  const [isEditing, setIsEditing] = useState(false);
  const [isPendingPublish, startPublish] = useTransition();
  const [isPendingClose, startClose] = useTransition();
  const [error, setError] = useState<string | null>(null);

  if (isEditing) {
    return (
      <EditProjectForm
        project={project}
        onCancel={() => setIsEditing(false)}
        onSaved={() => setIsEditing(false)}
      />
    );
  }

  const busy = isPendingPublish || isPendingClose;
  const canEdit =
    canManage && (project.status === "Draft" || project.status === "Open");
  const canPublish = canManage && project.status === "Draft";
  const canClose = canManage && project.status !== "Closed";

  function handlePublish() {
    setError(null);
    startPublish(async () => {
      const result = await publishProjectAction(project.id);
      if (result.error) setError(result.error);
    });
  }

  function handleClose() {
    if (!window.confirm("Close this project? This cannot be undone.")) return;
    setError(null);
    startClose(async () => {
      const result = await closeProjectAction(project.id);
      if (result.error) setError(result.error);
    });
  }

  return (
    <div className="flex flex-col gap-8">
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

        <div className="flex flex-wrap gap-x-5 gap-y-1 text-[13px] text-ink-soft">
          <span>Team size: {project.desiredTeamSize}</span>
          <span>Timeline: {project.timeline}</span>
          <span>Created by {project.createdByUser.displayName}</span>
          <span>{toRelativeDate(project.createdAt)}</span>
        </div>

        {canManage && (canEdit || canPublish || canClose) && (
          <div className="flex gap-2 pt-1">
            {canEdit && (
              <Button
                size="sm"
                variant="secondary"
                onClick={() => setIsEditing(true)}
                disabled={busy}
              >
                Edit
              </Button>
            )}
            {canPublish && (
              <Button size="sm" onClick={handlePublish} disabled={busy}>
                {isPendingPublish ? "Publishing…" : "Publish"}
              </Button>
            )}
            {canClose && (
              <Button
                size="sm"
                variant="destructive"
                onClick={handleClose}
                disabled={busy}
              >
                {isPendingClose ? "Closing…" : "Close project"}
              </Button>
            )}
          </div>
        )}

        {error && <p className="text-[13px] text-jubilee-red">{error}</p>}
      </div>

      <p className="text-[15px] text-ink leading-relaxed whitespace-pre-wrap">
        {project.description}
      </p>
    </div>
  );
}
