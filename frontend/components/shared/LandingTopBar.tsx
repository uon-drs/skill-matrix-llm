"use client";

import { useRouter } from "next/navigation";
import { signIn } from "next-auth/react";

import { Button } from "@/components/core";
import { useViewport } from "@/lib/hooks/useViewport";

/**
 * Sticky navigation bar for unauthenticated visitors.
 * Shows the UoN logo and Matchboard branding with Sign in / Sign up buttons.
 * Excludes search, notifications, and user-specific controls.
 */
export function LandingTopBar() {
  const router = useRouter();
  const { isMobile } = useViewport();

  return (
    <header
      className="sticky top-0 z-10 bg-paper border-b border-[var(--border)] h-[60px] flex items-center"
      style={{
        padding: isMobile ? "0 16px" : "0 28px",
        gap: isMobile ? 12 : 24,
      }}
    >
      {/* eslint-disable-next-line @next/next/no-img-element */}
      <img
        src="/uon-logo-blue.png"
        alt="University of Nottingham"
        className="object-contain"
        style={{ height: isMobile ? 28 : 34, width: "auto" }}
      />

      {!isMobile && (
        <>
          <span className="w-px h-7 bg-[var(--border-strong)] shrink-0" />
          <div className="flex items-center gap-2.5">
            <MatchboardMark size={22} />
            <span className="text-[18px] font-semibold tracking-[-0.025em] text-ink">
              Matchboard
            </span>
          </div>
          <div className="text-[13px] text-ink-soft shrink-0">
            Digital Research Service
          </div>
        </>
      )}

      {isMobile && (
        <div className="flex items-center gap-1.5">
          <MatchboardMark size={18} />
          <span className="text-[16px] font-semibold tracking-[-0.025em] text-ink">
            Matchboard
          </span>
        </div>
      )}

      <div className="flex-1" />

      <div className="flex items-center gap-2">
        <Button
          variant="secondary"
          size={isMobile ? "sm" : "md"}
          onClick={() => signIn("keycloak", { callbackUrl: "/dashboard" })}
        >
          Sign in
        </Button>
        <Button
          variant="primary"
          size={isMobile ? "sm" : "md"}
          onClick={() => router.push("/sign-up")}
        >
          Sign up
        </Button>
      </div>
    </header>
  );
}

function MatchboardMark({ size }: { size: number }) {
  return (
    <svg
      xmlns="http://www.w3.org/2000/svg"
      viewBox="0 0 24 24"
      width={size}
      height={size}
      aria-hidden="true"
    >
      <circle cx="9" cy="12" r="4" fill="#10263B" />
      <circle cx="17" cy="12" r="2.5" fill="#DEB406" />
    </svg>
  );
}
