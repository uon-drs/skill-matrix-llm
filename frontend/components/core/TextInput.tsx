"use client";

import type { ComponentType, InputHTMLAttributes } from "react";

import { cn } from "@/lib/utils";

interface TextInputProps extends InputHTMLAttributes<HTMLInputElement> {
  /** Visible label rendered above the input. */
  label?: string;
  /** Helper text rendered below the input. */
  help?: string;
  /** Optional leading icon component from `@heroicons/react/24/outline`. */
  icon?: ComponentType<{ className?: string }>;
}

/**
 * Labelled text input with optional leading icon and helper text.
 * Focuses with a Nottingham Blue border and outline ring.
 * @param label - Visible label above the input
 * @param help - Helper text below the input
 * @param icon - Optional leading icon component
 */
export function TextInput({
  label,
  help,
  icon: Icon,
  className,
  id,
  ...props
}: TextInputProps) {
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
      <div className="relative">
        {Icon && (
          <span className="absolute left-3 top-1/2 -translate-y-1/2 text-ink-muted flex pointer-events-none">
            <Icon className="w-4 h-4" />
          </span>
        )}
        <input
          id={inputId}
          className={cn(
            "w-full font-sans text-[14px] text-ink bg-paper rounded-sm box-border",
            "border border-[var(--border-strong)]",
            "outline-none transition-[border-color,outline-color] duration-[120ms] ease-[cubic-bezier(0.2,0,0,1)]",
            "focus:border-nottingham-blue focus:outline focus:outline-[1px] focus:outline-nottingham-blue focus:outline-offset-[-2px]",
            "placeholder:text-ink-faint",
            Icon ? "px-3 py-[9px] pl-9" : "px-3 py-[9px]",
            className,
          )}
          {...props}
        />
      </div>
      {help && <div className="text-[11.5px] text-ink-muted">{help}</div>}
    </div>
  );
}
