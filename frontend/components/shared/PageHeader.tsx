import type { ReactNode } from "react";

import { Eyebrow } from "@/components/core/Eyebrow";
import { cn } from "@/lib/utils";

interface PageHeaderProps {
  /** Small uppercase label rendered above the title. */
  eyebrow?: string;
  title: string;
  /** Supporting body text below the title. */
  description?: string;
  /** Monospaced metadata row (e.g. date, author, status). */
  meta?: ReactNode;
  /** Action slot displayed to the right of the title. */
  action?: ReactNode;
  className?: string;
}

/**
 * Responsive page-level header with optional eyebrow, description, meta row, and action slot.
 * Title scales: 24px (mobile) → 28px (≥900px) → 36px (≥1200px).
 * @param eyebrow - Small uppercase label above the title
 * @param title - Primary page heading
 * @param description - Supporting body text
 * @param meta - Monospaced metadata row
 * @param action - Optional CTA or action element
 */
export function PageHeader({
  eyebrow,
  title,
  description,
  meta,
  action,
  className,
}: PageHeaderProps) {
  return (
    <div className={cn("mb-8", className)}>
      {eyebrow && <Eyebrow className="mb-2.5">{eyebrow}</Eyebrow>}
      <div className="flex items-start gap-4">
        <div className="flex-1 min-w-0">
          <h1 className="font-display font-medium tracking-[-0.03em] leading-[1.05] text-ink m-0 text-[24px] md:text-[28px] lg:text-[36px]">
            {title}
          </h1>
          {description && (
            <p className="mt-3 text-[16px] text-ink-soft leading-[1.55] max-w-[640px]">
              {description}
            </p>
          )}
          {meta && (
            <div className="mt-3.5 font-mono text-[12px] text-ink-muted flex flex-wrap gap-4">
              {meta}
            </div>
          )}
        </div>
        {action && <div className="shrink-0">{action}</div>}
      </div>
    </div>
  );
}
