import type { HTMLAttributes, ReactNode } from "react";

import { cn } from "@/lib/utils";

type TagVariant = "skill" | "discipline" | "neutral";

interface TagProps extends HTMLAttributes<HTMLSpanElement> {
  /** Colour scheme; defaults to `"skill"`. */
  variant?: TagVariant;
  children: ReactNode;
}

const variantClasses: Record<TagVariant, string> = {
  skill: "bg-rebels-gold-20 text-nottingham-blue",
  discipline: "bg-nottingham-blue-20 text-nottingham-blue",
  neutral: "bg-portland-stone text-ink-soft",
};

/**
 * Label badge for categorising skills, disciplines, or neutral content.
 * Rendered in Geist Mono at 12px with a 2px radius.
 * @param variant - Colour scheme; defaults to `"skill"`
 */
export function Tag({
  variant = "skill",
  children,
  className,
  ...props
}: TagProps) {
  return (
    <span
      className={cn(
        "inline-flex items-center gap-1.5 px-[9px] py-[3px]",
        "rounded-[2px] text-[12px] font-mono",
        variantClasses[variant],
        className,
      )}
      {...props}
    >
      {children}
    </span>
  );
}
