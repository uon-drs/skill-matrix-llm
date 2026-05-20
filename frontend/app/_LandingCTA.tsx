import Link from "next/link";

import { signInWithKeycloak } from "./actions";

/**
 * Hero call-to-action buttons for the landing page.
 * Sign in uses a server action so the OAuth redirect is issued as an HTTP
 * response, avoiding client-side router state corruption on back navigation.
 */
export function LandingCTA() {
  return (
    <div className="flex items-center gap-3">
      <form action={signInWithKeycloak}>
        <button
          type="submit"
          className="inline-flex items-center gap-2 font-sans font-medium tracking-[-0.005em] rounded-sm px-[16px] py-[9px] text-[14px] bg-paper text-ink border border-[var(--border-strong)] hover:bg-portland-stone transition-colors duration-[120ms] cursor-pointer"
        >
          Sign in
        </button>
      </form>
      <Link
        href="/sign-up"
        className="inline-flex items-center gap-2 font-sans font-medium tracking-[-0.005em] rounded-sm px-[16px] py-[9px] text-[14px] bg-nottingham-blue text-paper border border-nottingham-blue hover:bg-[var(--color-primary-deep)] transition-colors duration-[120ms]"
      >
        Sign up
      </Link>
    </div>
  );
}
