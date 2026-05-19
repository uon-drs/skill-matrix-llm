"use client";

import type { HTMLAttributes } from "react";

import { cn } from "@/lib/utils";

type AvatarSize = "sm" | "md" | "lg" | "xl";

interface AvatarProps extends HTMLAttributes<HTMLSpanElement> {
  /** 1–2 character initials displayed inside the avatar. */
  initials: string;
  /** Gradient index (0–3); each maps to a UoN supporting-palette pairing. */
  hue?: 0 | 1 | 2 | 3;
  /** Preset dimensions; defaults to `"md"`. */
  size?: AvatarSize;
}

const AVATAR_GRADIENTS = [
  "linear-gradient(135deg, #DEB406, #B91C2E)", // Rebel's Gold → Jubilee Red
  "linear-gradient(135deg, #10263B, #37B4B0)", // Nottingham Blue → Trent Turquoise
  "linear-gradient(135deg, #792D85, #D7336C)", // Civic Purple → Pioneering Pink
  "linear-gradient(135deg, #005F36, #DEB406)", // Forest Green → Rebel's Gold
] as const;

const SIZE_PX: Record<AvatarSize, number> = {
  sm: 28,
  md: 40,
  lg: 56,
  xl: 72,
};

const FONT_SIZE: Record<AvatarSize, string> = {
  sm: "11px",
  md: "14px",
  lg: "18px",
  xl: "22px",
};

/**
 * User avatar with a UoN-palette gradient background and Paper-coloured initials.
 * @param initials - 1–2 character initials to display
 * @param hue - Gradient variant (0–3); defaults to `0`
 * @param size - Preset dimension; defaults to `"md"`
 */
export function Avatar({
  initials,
  hue = 0,
  size = "md",
  className,
  style,
  onClick,
  ...props
}: AvatarProps) {
  const px = SIZE_PX[size];
  return (
    <span
      className={cn(
        "inline-flex items-center justify-center font-medium tracking-[-0.01em]",
        "rounded-sm shrink-0 select-none text-paper",
        onClick ? "cursor-pointer" : "cursor-default",
        className,
      )}
      style={{
        width: px,
        height: px,
        background: AVATAR_GRADIENTS[hue % AVATAR_GRADIENTS.length],
        fontSize: FONT_SIZE[size],
        ...style,
      }}
      onClick={onClick}
      {...props}
    >
      {initials}
    </span>
  );
}
