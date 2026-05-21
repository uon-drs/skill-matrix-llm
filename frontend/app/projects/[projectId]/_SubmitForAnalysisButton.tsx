"use client";

import { useState, useTransition } from "react";

import { cn } from "@/lib/utils";

import { submitForAnalysis } from "./actions";

interface SubmitForAnalysisButtonProps {
  projectId: string;
  /** Disable the button when the project status does not permit reanalysis. */
  disabled?: boolean;
}

/**
 * Triggers LLM team analysis for a project by submitting it to the recommendation queue.
 * @param projectId - The project to submit
 * @param disabled - When true the button is inert (e.g. for Closed / TeamConfirmed projects)
 */
export function SubmitForAnalysisButton({
  projectId,
  disabled = false,
}: SubmitForAnalysisButtonProps) {
  const [isPending, startTransition] = useTransition();
  const [feedback, setFeedback] = useState<{
    type: "success" | "error";
    message: string;
  } | null>(null);

  function handleClick() {
    setFeedback(null);
    startTransition(async () => {
      const result = await submitForAnalysis(projectId);
      if (result.error) {
        setFeedback({ type: "error", message: result.error });
      } else {
        setFeedback({
          type: "success",
          message: "Submitted — analysis is queued and will complete shortly.",
        });
      }
    });
  }

  return (
    <div className="flex flex-col gap-2">
      <button
        onClick={handleClick}
        disabled={disabled || isPending}
        className={cn(
          "px-4 py-2 text-[14px] font-medium rounded-sm transition-colors duration-[120ms]",
          disabled || isPending
            ? "bg-portland-stone text-ink-muted cursor-not-allowed"
            : "bg-nottingham-blue text-paper hover:bg-nottingham-blue/90",
        )}
      >
        {isPending ? "Submitting…" : "Submit for analysis"}
      </button>
      {feedback && (
        <p
          className={cn(
            "text-[13px]",
            feedback.type === "success" ? "text-green-700" : "text-red-600",
          )}
        >
          {feedback.message}
        </p>
      )}
    </div>
  );
}
