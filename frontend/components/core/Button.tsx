"use client";

import type { ButtonHTMLAttributes, ComponentType } from "react";

import { cn } from "@/lib/utils";

type ButtonVariant = "primary" | "secondary" | "ghost" | "destructive";
type ButtonSize = "sm" | "md" | "lg";

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  /** Visual style; defaults to `"primary"`. */
  variant?: ButtonVariant;
  /** Button dimensions; defaults to `"md"`. */
  size?: ButtonSize;
  /** Leading icon component from `@heroicons/react/24/outline`. */
  icon?: ComponentType<{ className?: string }>;
  /** Trailing icon component from `@heroicons/react/24/outline`. */
  iconRight?: ComponentType<{ className?: string }>;
}

const variantClasses: Record<ButtonVariant, string> = {
  primary:
    "bg-nottingham-blue text-paper border border-nottingham-blue hover:bg-[var(--color-primary-deep)]",
  secondary:
    "bg-paper text-ink border border-[var(--border-strong)] hover:bg-portland-stone",
  ghost:
    "bg-transparent text-ink border border-transparent hover:bg-[rgba(16,38,59,0.06)]",
  destructive:
    "bg-paper text-jubilee-red border border-[rgba(185,28,46,0.4)] hover:bg-jubilee-red-20",
};

const sizeClasses: Record<ButtonSize, string> = {
  sm: "px-[10px] py-[5px] text-[12px] [&_svg]:w-[14px] [&_svg]:h-[14px]",
  md: "px-[16px] py-[9px] text-[14px] [&_svg]:w-[16px] [&_svg]:h-[16px]",
  lg: "px-[20px] py-[12px] text-[15px] [&_svg]:w-[16px] [&_svg]:h-[16px]",
};

/**
 * Primary interaction button with four visual variants and three sizes.
 * Icon-only buttons must provide an `aria-label`.
 * @param variant - Visual style; defaults to `"primary"`
 * @param size - Button dimensions; defaults to `"md"`
 * @param icon - Optional leading icon component
 * @param iconRight - Optional trailing icon component
 */
export function Button({
  variant = "primary",
  size = "md",
  icon: Icon,
  iconRight: IconRight,
  children,
  className,
  disabled,
  type = "button",
  ...props
}: ButtonProps) {
  return (
    <button
      type={type}
      disabled={disabled}
      className={cn(
        "inline-flex items-center gap-2 font-sans font-medium tracking-[-0.005em] rounded-sm cursor-pointer",
        "transition-[background-color,transform,border-color] duration-[120ms] ease-[cubic-bezier(0.2,0,0,1)]",
        "active:scale-[0.98] active:transition-[transform] active:duration-[80ms]",
        "disabled:opacity-50 disabled:cursor-not-allowed",
        variantClasses[variant],
        sizeClasses[size],
        className,
      )}
      {...props}
    >
      {Icon && <Icon className="shrink-0" />}
      {children}
      {IconRight && <IconRight className="shrink-0" />}
    </button>
  );
}
