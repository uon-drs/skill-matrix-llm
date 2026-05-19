"use client";

import type { HTMLAttributes, ReactNode } from "react";

import { cn } from "@/lib/utils";

interface ChipProps extends HTMLAttributes<HTMLSpanElement> {
  /** Whether the chip is in its selected/active state. */
  active?: boolean;
  children: ReactNode;
}

/**
 * Filter pill toggle. Active state uses Nottingham Blue background with Paper text.
 * @param active - Whether the chip is selected
 */
export function Chip({ active, children, className, ...props }: ChipProps) {
  return (
    <span
      className={cn(
        "inline-flex items-center gap-1.5 px-3 py-[5px]",
        "rounded-pill text-[13px] border cursor-pointer select-none",
        "transition-all duration-[120ms] ease-[cubic-bezier(0.2,0,0,1)]",
        active
          ? "bg-nottingham-blue text-paper border-nottingham-blue font-medium"
          : "bg-paper text-ink border-[var(--border)] font-normal hover:border-[var(--border-strong)]",
        className,
      )}
      role="button"
      {...props}
    >
      {children}
    </span>
  );
}
