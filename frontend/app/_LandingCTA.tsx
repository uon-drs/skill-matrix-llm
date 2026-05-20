"use client";

import Link from "next/link";
import { signIn } from "next-auth/react";

/**
 * Hero call-to-action buttons for the landing page.
 * Extracted as a client component so the parent server component can keep
 * its server-side session check while these buttons call next-auth.
 */
export function LandingCTA() {
  return (
    <div className="flex items-center gap-3">
      <button
        onClick={() => signIn("keycloak", { callbackUrl: "/dashboard" })}
        className="inline-flex items-center gap-2 font-sans font-medium tracking-[-0.005em] rounded-sm px-[16px] py-[9px] text-[14px] bg-paper text-ink border border-[var(--border-strong)] hover:bg-portland-stone transition-colors duration-[120ms] cursor-pointer"
      >
        Sign in
      </button>
      <Link
        href="/sign-up"
        className="inline-flex items-center gap-2 font-sans font-medium tracking-[-0.005em] rounded-sm px-[16px] py-[9px] text-[14px] bg-nottingham-blue text-paper border border-nottingham-blue hover:bg-[var(--color-primary-deep)] transition-colors duration-[120ms]"
      >
        Sign up
      </Link>
    </div>
  );
}
