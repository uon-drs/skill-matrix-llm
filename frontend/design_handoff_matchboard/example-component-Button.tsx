// example-component-Button.tsx
//
// Sample port of the prototype's `Button` component (ui-kit-reference/components.jsx)
// into a production-ready Next.js + TypeScript file.
//
// Pattern to follow for the rest of the components:
//   - Named export (no `Object.assign(window, ...)`).
//   - ES module imports.
//   - Typed props.
//   - Keep CSS variables — they are the source of truth.
//   - Tailwind classes are optional; this example uses inline style to mirror
//     the prototype 1:1 so visual fidelity is exact.

"use client";

import { type MouseEventHandler, type ReactNode, useState } from "react";

type ButtonVariant = "primary" | "secondary" | "ghost" | "destructive";
type ButtonSize = "sm" | "md" | "lg";

interface ButtonProps {
  variant?: ButtonVariant;
  size?: ButtonSize;
  icon?: ReactNode; // pass a Heroicon component
  iconRight?: ReactNode;
  children?: ReactNode;
  onClick?: MouseEventHandler<HTMLButtonElement>;
  type?: "button" | "submit" | "reset";
  disabled?: boolean;
  className?: string;
  "aria-label"?: string;
}

const VARIANT_STYLES: Record<
  ButtonVariant,
  { background: string; color: string; border: string }
> = {
  primary: {
    background: "var(--color-nottingham-blue)",
    color: "var(--color-paper)",
    border: "1px solid var(--color-nottingham-blue)",
  },
  secondary: {
    background: "var(--color-paper)",
    color: "var(--color-nottingham-blue)",
    border: "1px solid var(--border-strong)",
  },
  ghost: {
    background: "transparent",
    color: "var(--color-nottingham-blue)",
    border: "1px solid transparent",
  },
  destructive: {
    background: "var(--color-paper)",
    color: "var(--color-jubilee-red)",
    border: "1px solid rgba(185,28,46,0.4)",
  },
};

const HOVER_BG: Record<ButtonVariant, string> = {
  primary: "#081827", // darker than palette — confirm with brand team
  secondary: "var(--color-portland-stone)",
  ghost: "rgba(16,38,59,0.06)",
  destructive: "var(--color-jubilee-red-20)",
};

export function Button({
  variant = "primary",
  size = "md",
  icon,
  iconRight,
  children,
  onClick,
  type = "button",
  disabled = false,
  className,
  ...rest
}: ButtonProps) {
  const [hover, setHover] = useState(false);
  const [press, setPress] = useState(false);

  const base = VARIANT_STYLES[variant];
  const padding =
    size === "sm" ? "5px 10px" : size === "lg" ? "12px 20px" : "9px 16px";
  const fontSize = size === "sm" ? 12 : size === "lg" ? 15 : 14;
  const iconSize = size === "sm" ? 14 : 16;

  return (
    <button
      type={type}
      disabled={disabled}
      onMouseEnter={() => setHover(true)}
      onMouseLeave={() => {
        setHover(false);
        setPress(false);
      }}
      onMouseDown={() => setPress(true)}
      onMouseUp={() => setPress(false)}
      onClick={onClick}
      className={className}
      {...rest}
      style={{
        ...base,
        background: hover ? HOVER_BG[variant] : base.background,
        display: "inline-flex",
        alignItems: "center",
        gap: 8,
        padding,
        fontFamily: "var(--font-sans)",
        fontSize,
        fontWeight: 500,
        letterSpacing: "-0.005em",
        borderRadius: 4,
        cursor: disabled ? "not-allowed" : "pointer",
        opacity: disabled ? 0.5 : 1,
        transform: press ? "scale(0.98)" : "scale(1)",
        transition:
          "background 120ms cubic-bezier(0.2,0,0,1), " +
          "transform 80ms cubic-bezier(0.2,0,0,1), " +
          "border-color 120ms",
      }}
    >
      {icon && (
        <span
          style={{ width: iconSize, height: iconSize, display: "inline-flex" }}
        >
          {icon}
        </span>
      )}
      {children}
      {iconRight && (
        <span
          style={{ width: iconSize, height: iconSize, display: "inline-flex" }}
        >
          {iconRight}
        </span>
      )}
    </button>
  );
}

/*
   Usage:

   import { Button } from "@/components/Button";
   import { PlusIcon, ArrowRightIcon } from "@heroicons/react/24/outline";

   <Button variant="primary" icon={<PlusIcon className="w-4 h-4"/>}>
     Post project
   </Button>

   <Button variant="primary" size="lg" iconRight={<ArrowRightIcon className="w-4 h-4"/>}>
     Request to join
   </Button>

   <Button variant="destructive">Withdraw project</Button>
*/
