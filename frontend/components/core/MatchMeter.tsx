import { cn } from "@/lib/utils";

interface MatchMeterProps {
  /** Match percentage (0–100). */
  pct: number;
  /** Bar width in pixels; defaults to `120`. */
  width?: number;
  className?: string;
}

/**
 * Horizontal progress bar showing a match percentage with colour-coded fill.
 * Fill colour: Forest Green (≥70%), Rebel's Gold (≥45%), muted otherwise.
 * @param pct - Match percentage (0–100)
 * @param width - Bar width in pixels; defaults to `120`
 */
export function MatchMeter({ pct, width = 120, className }: MatchMeterProps) {
  const fillColor =
    pct >= 70
      ? "var(--color-forest-green)"
      : pct >= 45
        ? "var(--color-rebels-gold)"
        : "var(--color-ink-faint)";

  return (
    <div className={cn("inline-flex items-center gap-2", className)}>
      <div
        className="h-[6px] rounded-[2px] overflow-hidden bg-portland-stone shrink-0"
        style={{ width }}
      >
        <div
          className="h-full rounded-[2px] transition-[width] duration-[240ms] ease-[cubic-bezier(0.2,0,0,1)]"
          style={{ width: `${pct}%`, background: fillColor }}
        />
      </div>
      <span className="font-mono text-[12px] font-medium text-ink tabular-nums text-right min-w-[32px]">
        {pct}%
      </span>
    </div>
  );
}
