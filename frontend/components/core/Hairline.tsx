import type { HTMLAttributes } from "react";

import { cn } from "@/lib/utils";

interface HairlineProps extends HTMLAttributes<HTMLDivElement> {
  /** Renders as a vertical 1px line instead of a horizontal one. */
  vertical?: boolean;
}

/**
 * 1px visual separator; horizontal by default.
 * @param vertical - Render as a vertical divider instead
 */
export function Hairline({ vertical, className, ...props }: HairlineProps) {
  return (
    <div
      className={cn(
        "bg-[var(--border)]",
        vertical ? "w-px self-stretch" : "h-px w-full",
        className,
      )}
      {...props}
    />
  );
}
