"use client";

import { useState, useTransition } from "react";

import { cn } from "@/lib/utils";

import { createProject } from "./actions";

/**
 * Form for creating a new project. Calls the createProject server action on submit
 * and redirects to /projects on success.
 */
export function CreateProjectForm() {
  const [isPending, startTransition] = useTransition();
  const [error, setError] = useState<string | null>(null);

  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [desiredTeamSize, setDesiredTeamSize] = useState<number | "">(1);
  const [timeline, setTimeline] = useState("");

  const isValid =
    title.trim().length > 0 &&
    description.trim().length > 0 &&
    typeof desiredTeamSize === "number" &&
    desiredTeamSize >= 1 &&
    timeline.trim().length > 0;

  function handleSubmit() {
    if (!isValid) return;
    setError(null);
    startTransition(async () => {
      try {
        await createProject(
          title.trim(),
          description.trim(),
          desiredTeamSize as number,
          timeline.trim(),
        );
      } catch (err) {
        setError(`Failed to create project. Please try again. ${err}`);
      }
    });
  }

  return (
    <div className="flex flex-col gap-5">
      <Field label="Title">
        <input
          type="text"
          placeholder="e.g. Library management system"
          maxLength={256}
          value={title}
          onChange={(e) => setTitle(e.target.value)}
          disabled={isPending}
          className={inputClass}
        />
      </Field>

      <Field label="Description">
        <textarea
          placeholder="Describe the project's goals and requirements…"
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
          placeholder="e.g. 8 weeks, Q3 2026"
          maxLength={256}
          value={timeline}
          onChange={(e) => setTimeline(e.target.value)}
          disabled={isPending}
          className={inputClass}
        />
      </Field>

      {error && <p className="text-[13px] text-jubilee-red">{error}</p>}

      <div className="pt-1">
        <button
          type="button"
          onClick={handleSubmit}
          disabled={!isValid || isPending}
          className={cn(
            "px-5 py-[10px] text-[14px] font-medium rounded-sm",
            "bg-nottingham-blue text-paper",
            "hover:bg-nottingham-blue/90 transition-colors duration-[120ms]",
            "disabled:opacity-40 disabled:cursor-not-allowed",
          )}
        >
          {isPending ? "Creating…" : "Create project"}
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
