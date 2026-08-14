"use client";

import { useState, useTransition } from "react";

import type { ProjectDetail } from "@/lib/api/types";
import { cn } from "@/lib/utils";

import { updateProjectAction } from "./actions";

interface EditProjectFormProps {
  project: ProjectDetail;
  onCancel: () => void;
  onSaved: () => void;
}

/**
 * Inline form for editing a project's title, description, desired team size, and timeline.
 * @param project - The project to edit, used to pre-populate fields
 * @param onCancel - Called when editing is cancelled without saving
 * @param onSaved - Called after a successful save
 */
export function EditProjectForm({
  project,
  onCancel,
  onSaved,
}: EditProjectFormProps) {
  const [isPending, startTransition] = useTransition();
  const [error, setError] = useState<string | null>(null);

  const [title, setTitle] = useState(project.title);
  const [description, setDescription] = useState(project.description);
  const [desiredTeamSize, setDesiredTeamSize] = useState<number | "">(
    project.desiredTeamSize,
  );
  const [timeline, setTimeline] = useState(project.timeline);

  const isValid =
    title.trim().length > 0 &&
    description.trim().length > 0 &&
    typeof desiredTeamSize === "number" &&
    desiredTeamSize >= 1 &&
    timeline.trim().length > 0;

  function handleSave() {
    if (!isValid) return;
    setError(null);
    startTransition(async () => {
      const result = await updateProjectAction(project.id, {
        title: title.trim(),
        description: description.trim(),
        desiredTeamSize: desiredTeamSize as number,
        timeline: timeline.trim(),
      });
      if (result.error) setError(result.error);
      else onSaved();
    });
  }

  return (
    <div className="flex flex-col gap-5">
      <Field label="Title">
        <input
          type="text"
          maxLength={256}
          value={title}
          onChange={(e) => setTitle(e.target.value)}
          disabled={isPending}
          className={inputClass}
        />
      </Field>

      <Field label="Description">
        <textarea
          rows={4}
          value={description}
          onChange={(e) => setDescription(e.target.value)}
          disabled={isPending}
          className={cn(inputClass, "resize-y")}
        />
      </Field>

      <Field label="Desired team size">
        <input
          type="number"
          min={1}
          value={desiredTeamSize}
          onChange={(e) =>
            setDesiredTeamSize(
              e.target.value === "" ? "" : parseInt(e.target.value, 10),
            )
          }
          disabled={isPending}
          className={cn(inputClass, "w-32")}
        />
      </Field>

      <Field label="Timeline">
        <input
          type="text"
          maxLength={256}
          value={timeline}
          onChange={(e) => setTimeline(e.target.value)}
          disabled={isPending}
          className={inputClass}
        />
      </Field>

      {error && <p className="text-[13px] text-jubilee-red">{error}</p>}

      <div className="flex gap-2 pt-1">
        <button
          type="button"
          onClick={handleSave}
          disabled={!isValid || isPending}
          className={cn(
            "px-5 py-[10px] text-[14px] font-medium rounded-sm",
            "bg-nottingham-blue text-paper",
            "hover:bg-nottingham-blue/90 transition-colors duration-[120ms]",
            "disabled:opacity-40 disabled:cursor-not-allowed",
          )}
        >
          {isPending ? "Saving…" : "Save"}
        </button>
        <button
          type="button"
          onClick={onCancel}
          disabled={isPending}
          className={cn(
            "px-5 py-[10px] text-[14px] font-medium rounded-sm",
            "bg-paper text-ink border border-[var(--border-strong)]",
            "hover:bg-portland-stone transition-colors duration-[120ms]",
            "disabled:opacity-40 disabled:cursor-not-allowed",
          )}
        >
          Cancel
        </button>
      </div>
    </div>
  );
}

function Field({
  label,
  children,
}: {
  label: string;
  children: React.ReactNode;
}) {
  return (
    <div className="flex flex-col gap-1.5">
      <label className="text-[13px] font-medium text-ink-soft">{label}</label>
      {children}
    </div>
  );
}

const inputClass = cn(
  "w-full font-sans text-[14px] text-ink border border-[var(--border)] rounded-sm px-3 py-[9px]",
  "bg-paper placeholder:text-ink-faint",
  "focus:outline-none focus:border-nottingham-blue focus:ring-2 focus:ring-nottingham-blue/20",
  "transition-[border-color,box-shadow] duration-[120ms] disabled:opacity-50",
);
