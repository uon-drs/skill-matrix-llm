"use client";

import type { TextareaHTMLAttributes } from "react";

import { cn } from "@/lib/utils";

interface TextareaProps extends TextareaHTMLAttributes<HTMLTextAreaElement> {
  /** Visible label rendered above the textarea. */
  label?: string;
  /** Helper text rendered below the textarea. */
  help?: string;
}

/**
 * Labelled multi-line text input with optional helper text.
 * Focuses with a Nottingham Blue border and outline ring.
 * @param label - Visible label above the textarea
 * @param help - Helper text below the textarea
 * @param rows - Number of visible rows; defaults to `4`
 */
export function Textarea({
  label,
  help,
  className,
  rows = 4,
  id,
  ...props
}: TextareaProps) {
  const inputId = id ?? label?.toLowerCase().replace(/\s+/g, "-");
  return (
    <div className="flex flex-col gap-1.5">
      {label && (
        <label
          htmlFor={inputId}
          className="text-[12px] font-medium text-ink-soft"
        >
          {label}
        </label>
      )}
      <textarea
        id={inputId}
        rows={rows}
        className={cn(
          "w-full font-sans text-[14px] text-ink bg-paper rounded-sm box-border",
          "border border-[var(--border-strong)] resize-y leading-[1.5]",
          "outline-none transition-[border-color,outline-color] duration-[120ms] ease-[cubic-bezier(0.2,0,0,1)]",
          "focus:border-nottingham-blue focus:outline focus:outline-[1px] focus:outline-nottingham-blue focus:outline-offset-[-2px]",
          "placeholder:text-ink-faint px-3 py-[10px]",
          className,
        )}
        {...props}
      />
      {help && <div className="text-[11.5px] text-ink-muted">{help}</div>}
    </div>
  );
}
