import { signInWithKeycloak, signUpWithKeycloak } from "./actions";

/**
 * Hero call-to-action buttons for the landing page.
 * Both actions use server actions so OAuth redirects are issued as HTTP
 * responses, avoiding client-side router state corruption on back navigation.
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
      <form action={signUpWithKeycloak}>
        <button
          type="submit"
          className="inline-flex items-center gap-2 font-sans font-medium tracking-[-0.005em] rounded-sm px-[16px] py-[9px] text-[14px] bg-nottingham-blue text-paper border border-nottingham-blue hover:bg-[var(--color-primary-deep)] transition-colors duration-[120ms] cursor-pointer"
        >
          Sign up
        </button>
      </form>
    </div>
  );
}
