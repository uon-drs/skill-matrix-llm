import type { HTMLAttributes, ReactNode } from "react";

import { cn } from "@/lib/utils";

interface EyebrowProps extends HTMLAttributes<HTMLDivElement> {
  children: ReactNode;
}

/**
 * Small uppercase label used above section titles or content blocks.
 * @param children - Label text
 */
export function Eyebrow({ children, className, ...props }: EyebrowProps) {
  return (
    <div
      className={cn(
        "text-[11px] font-medium tracking-[0.14em] uppercase text-ink-muted",
        className,
      )}
      {...props}
    >
      {children}
    </div>
  );
}
