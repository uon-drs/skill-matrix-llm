import { cn } from "@/lib/utils";

interface MatchBadgeProps {
  /** Match percentage (0–100). */
  pct: number;
  className?: string;
}

/**
 * Compact inline pill displaying a match percentage in Forest Green.
 * @param pct - Match percentage (0–100)
 */
export function MatchBadge({ pct, className }: MatchBadgeProps) {
  return (
    <span
      className={cn(
        "inline-flex items-center gap-1 px-[9px] py-[3px]",
        "bg-forest-green-20 text-forest-green",
        "rounded-sm font-mono text-[11.5px] font-medium",
        className,
      )}
    >
      {pct}% match
    </span>
  );
}
