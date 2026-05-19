import { MagnifyingGlassIcon } from "@heroicons/react/24/outline";
import type { ComponentType, ReactNode } from "react";

import { cn } from "@/lib/utils";

interface EmptyStateProps {
  /** Icon component to display; defaults to `MagnifyingGlassIcon`. */
  icon?: ComponentType<{ className?: string }>;
  title: string;
  /** Supporting description below the title. */
  description?: string;
  /** Optional CTA element rendered below the description. */
  action?: ReactNode;
  className?: string;
}

/**
 * Centred empty state with a circular icon background, title, description, and optional CTA.
 * @param icon - Heroicon component to display; defaults to `MagnifyingGlassIcon`
 * @param title - Primary heading
 * @param description - Supporting body text
 * @param action - Optional call-to-action element
 */
export function EmptyState({
  icon: Icon = MagnifyingGlassIcon,
  title,
  description,
  action,
  className,
}: EmptyStateProps) {
  return (
    <div
      className={cn(
        "text-center py-16 px-6",
        "border border-dashed border-[var(--border-strong)] rounded-md",
        "text-ink-soft",
        className,
      )}
    >
      <div className="inline-flex p-3.5 rounded-full bg-portland-stone mb-4">
        <Icon className="w-5 h-5 text-ink-muted" />
      </div>
      <h3 className="text-[18px] font-medium text-ink m-0 tracking-[-0.01em]">
        {title}
      </h3>
      {description && (
        <p className="text-[14px] text-ink-soft mt-2 mb-4 mx-auto max-w-[380px] leading-[1.5]">
          {description}
        </p>
      )}
      {action}
    </div>
  );
}
