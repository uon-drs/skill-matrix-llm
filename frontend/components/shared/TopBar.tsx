"use client";

import {
  ArrowRightStartOnRectangleIcon,
  Bars3Icon,
  BellIcon,
  MagnifyingGlassIcon,
  PlusIcon,
} from "@heroicons/react/24/outline";
import Link from "next/link";
import { useState } from "react";

import { Avatar } from "@/components/core/Avatar";
import { Button } from "@/components/core/Button";
import { useViewport } from "@/lib/hooks/useViewport";

interface TopBarProps {
  /** Active route identifier. */
  route?: string;
  /** User initials displayed in the avatar. */
  userInitials: string;
  /** Avatar gradient hue (0–3); defaults to `1`. */
  userHue?: 0 | 1 | 2 | 3;
  onLogo?: () => void;
  onNavigate?: (route: string) => void;
  onMenuToggle?: () => void;
  onSignOut?: () => void;
}

/**
 * Sticky 60px application header.
 * Desktop: UoN logo → divider → Matchboard wordmark → DRS label → search → Post button → bell → avatar.
 * Mobile: hamburger → logo → compact wordmark → Post icon → avatar.
 * @param userInitials - Initials shown in the top-right avatar
 * @param userHue - Avatar gradient variant (0–3)
 */
export function TopBar({
  userInitials,
  userHue = 1,
  onLogo,
  onNavigate,
  onMenuToggle,
  onSignOut,
}: TopBarProps) {
  const [search, setSearch] = useState("");
  const { isMobile } = useViewport();

  return (
    <header
      className="sticky top-0 z-10 bg-paper border-b border-[var(--border)] h-[60px] flex items-center"
      style={{
        padding: isMobile ? "0 16px" : "0 28px",
        gap: isMobile ? 12 : 24,
      }}
    >
      {isMobile && (
        <button
          aria-label="Open navigation"
          onClick={onMenuToggle}
          className="-ml-1.5 w-9 h-9 rounded-sm flex items-center justify-center text-ink border border-transparent bg-transparent cursor-pointer hover:bg-[rgba(16,38,59,0.06)]"
        >
          <Bars3Icon className="w-[22px] h-[22px]" />
        </button>
      )}

      {/* eslint-disable-next-line @next/next/no-img-element */}
      <img
        src="/uon-logo-blue.png"
        alt="University of Nottingham"
        className="cursor-pointer object-contain"
        style={{ height: isMobile ? 28 : 34, width: "auto" }}
        onClick={onLogo}
      />

      {!isMobile && (
        <>
          <span className="w-px h-7 bg-[var(--border-strong)] shrink-0" />
          <button
            className="flex items-center gap-2.5 cursor-pointer bg-transparent border-none p-0"
            onClick={onLogo}
          >
            <MatchboardMark size={22} />
            <span className="text-[18px] font-semibold tracking-[-0.025em] text-ink">
              Matchboard
            </span>
          </button>
          <div className="text-[13px] text-ink-soft shrink-0">
            Digital Research Service
          </div>
        </>
      )}

      {isMobile && (
        <button
          className="flex items-center gap-1.5 cursor-pointer bg-transparent border-none p-0"
          onClick={onLogo}
        >
          <MatchboardMark size={18} />
          <span className="text-[16px] font-semibold tracking-[-0.025em] text-ink">
            Matchboard
          </span>
        </button>
      )}

      {!isMobile && (
        <div className="flex-1 max-w-[460px] ml-3 relative">
          <span className="absolute left-3 top-1/2 -translate-y-1/2 text-ink-muted flex pointer-events-none">
            <MagnifyingGlassIcon className="w-4 h-4" />
          </span>
          <input
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            placeholder="Search researchers, projects, skills"
            className="w-full py-2 pr-3 pl-9 bg-portland-stone border border-transparent rounded-md font-sans text-[14px] text-ink outline-none placeholder:text-ink-faint box-border"
          />
        </div>
      )}

      <div className="flex-1" />

      {isMobile ? (
        <Link
          href="/projects/new"
          aria-label="Post project"
          className="w-9 h-9 rounded-sm flex items-center justify-center bg-nottingham-blue text-paper"
        >
          <PlusIcon className="w-[18px] h-[18px]" />
        </Link>
      ) : (
        <Link href="/projects/new">
          <Button variant="primary" size="md" icon={PlusIcon}>
            Post project
          </Button>
        </Link>
      )}

      {!isMobile && (
        <button
          aria-label="View notifications"
          className="w-9 h-9 rounded-sm flex items-center justify-center text-ink-soft bg-transparent border border-transparent cursor-pointer hover:bg-[rgba(16,38,59,0.06)] transition-colors duration-[120ms]"
        >
          <BellIcon className="w-5 h-5" />
        </button>
      )}

      {onSignOut && (
        <button
          onClick={onSignOut}
          className="flex items-center gap-1.5 px-3 h-9 rounded-sm text-[14px] text-ink-soft bg-transparent border border-transparent cursor-pointer hover:bg-[rgba(16,38,59,0.06)] transition-colors duration-[120ms]"
        >
          <ArrowRightStartOnRectangleIcon className="w-4 h-4 shrink-0" />
          {!isMobile && <span>Log out</span>}
        </button>
      )}

      <Avatar
        initials={userInitials}
        hue={userHue}
        size="sm"
        onClick={() => onNavigate?.("profile-me")}
      />
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
