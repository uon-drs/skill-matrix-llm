import { cn } from "@/lib/utils";

type StatusType = "open" | "pending" | "urgent" | "closed" | "accepted";

interface StatusPillProps {
  status: StatusType;
  /** Optional label override; falls back to the status default. */
  label?: string;
  className?: string;
}

const STATUS_CONFIG: Record<
  StatusType,
  { color: string; defaultLabel: string }
> = {
  open: { color: "var(--color-forest-green)", defaultLabel: "Open" },
  pending: {
    color: "var(--color-mandarin-orange)",
    defaultLabel: "Awaiting review",
  },
  urgent: {
    color: "var(--color-jubilee-red)",
    defaultLabel: "Closing today",
  },
  closed: { color: "var(--color-ink-muted)", defaultLabel: "Closed" },
  accepted: { color: "var(--color-forest-green)", defaultLabel: "Accepted" },
};

/**
 * Coloured dot with a label indicating project or request status.
 * @param status - One of the defined status states
 * @param label - Optional override for the default status label
 */
export function StatusPill({ status, label, className }: StatusPillProps) {
  const config = STATUS_CONFIG[status];
  return (
    <span
      className={cn(
        "inline-flex items-center gap-1.5 font-mono text-[11.5px] font-medium",
        className,
      )}
      style={{ color: config.color }}
    >
      <span
        className="w-2 h-2 rounded-full shrink-0"
        style={{ background: config.color }}
      />
      {label ?? config.defaultLabel}
    </span>
  );
}
