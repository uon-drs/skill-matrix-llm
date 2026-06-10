"use client";

import {
  BeakerIcon,
  BookmarkIcon,
  InboxIcon,
  SparklesIcon,
  UsersIcon,
  XMarkIcon,
} from "@heroicons/react/24/outline";
import { useEffect, useRef } from "react";

import { Eyebrow } from "@/components/core/Eyebrow";
import { Hairline } from "@/components/core/Hairline";
import { useViewport } from "@/lib/hooks/useViewport";
import { cn } from "@/lib/utils";

interface NavItem {
  id: string;
  label: string;
  Icon: React.ComponentType<{ className?: string }>;
  badge?: number;
}

const NAV_ITEMS: NavItem[] = [
  { id: "discover", label: "Discover", Icon: SparklesIcon },
  { id: "matches", label: "Invitations", Icon: InboxIcon },
  { id: "my-projects", label: "My projects", Icon: BeakerIcon },
  { id: "saved", label: "Saved", Icon: BookmarkIcon },
  { id: "network", label: "Network", Icon: UsersIcon },
];

const DISCIPLINES = [
  "Materials Science",
  "Bioengineering",
  "Computational Biology",
  "Climate Science",
  "Ethics",
];

interface LeftRailProps {
  /** Currently active route identifier. */
  route?: string;
  onNavigate?: (route: string) => void;
  /** Controls drawer visibility on mobile. */
  open?: boolean;
  /** Called when the drawer should close (route change or scrim click). */
  onClose?: () => void;
  /** Number of pending invitations to show as a badge on the Invitations nav item. */
  pendingInviteCount?: number;
}

/**
 * Primary navigation sidebar.
 * Desktop: 220px sticky rail with nav links and a disciplines section.
 * Mobile: 280px slide-out drawer (180ms ease-out) with scrim overlay;
 * closes automatically when the active route changes.
 * @param route - Active route identifier
 * @param open - Drawer visibility on mobile
 * @param onClose - Callback to close the drawer
 */
export function LeftRail({
  route,
  onNavigate,
  open = false,
  onClose,
  pendingInviteCount,
}: LeftRailProps) {
  const { isMobile } = useViewport();
  const prevRouteRef = useRef(route);

  useEffect(() => {
    if (route !== prevRouteRef.current) {
      prevRouteRef.current = route;
      if (isMobile && open) onClose?.();
    }
  }, [route, isMobile, open, onClose]);

  const railContent = (
    <>
      <NavList
        route={route}
        onNavigate={onNavigate}
        pendingInviteCount={pendingInviteCount}
      />
      <div className="h-6" />
      <Hairline className="w-[calc(100%-12px)]" />
      <div className="h-4" />
      <Eyebrow className="px-3 mb-2">Disciplines</Eyebrow>
      <div className="flex flex-col gap-px">
        {DISCIPLINES.map((d) => (
          <div
            key={d}
            className="px-3 py-1.5 text-[13px] text-ink-soft cursor-pointer rounded-sm hover:bg-[rgba(16,38,59,0.04)] transition-colors duration-[120ms]"
          >
            {d}
          </div>
        ))}
      </div>
    </>
  );

  if (isMobile) {
    return (
      <>
        {open && (
          <div
            onClick={onClose}
            className="fixed inset-0 bg-[rgba(16,38,59,0.4)] z-20 animate-[mb-fade_180ms_cubic-bezier(0.2,0,0,1)]"
          />
        )}
        <nav
          className={cn(
            "fixed top-0 left-0 bottom-0 z-30 overflow-y-auto",
            "bg-paper border-r border-[var(--border)]",
            "transition-[transform,box-shadow] duration-[220ms] ease-[cubic-bezier(0.2,0,0,1)]",
            open ? "shadow-modal" : "",
          )}
          style={{
            width: 280,
            maxWidth: "85vw",
            padding: "20px 16px",
            transform: open ? "translateX(0)" : "translateX(-100%)",
          }}
        >
          <div className="flex items-center justify-between mb-[18px]">
            {/* eslint-disable-next-line @next/next/no-img-element */}
            <img
              src="/uon-logo-blue.png"
              alt="University of Nottingham"
              className="h-7"
            />
            <button
              aria-label="Close navigation"
              onClick={onClose}
              className="w-8 h-8 rounded-sm flex items-center justify-center text-ink-soft bg-transparent border-none cursor-pointer hover:bg-[rgba(16,38,59,0.06)]"
            >
              <XMarkIcon className="w-[18px] h-[18px]" />
            </button>
          </div>
          {railContent}
        </nav>
        <style>{`@keyframes mb-fade { from { opacity: 0; } to { opacity: 1; } }`}</style>
      </>
    );
  }

  return (
    <nav
      className="shrink-0 border-r border-[var(--border)] min-h-[calc(100vh-60px)] sticky top-[60px] self-start overflow-y-auto"
      style={{ width: 220, padding: "24px 0 24px 16px" }}
    >
      {railContent}
    </nav>
  );
}

function NavList({
  route,
  onNavigate,
  pendingInviteCount,
}: {
  route?: string;
  onNavigate?: (route: string) => void;
  pendingInviteCount?: number;
}) {
  return (
    <div className="flex flex-col gap-0.5">
      {NAV_ITEMS.map((item) => {
        const active = route === item.id;
        const badge =
          item.id === "matches" && pendingInviteCount
            ? pendingInviteCount
            : undefined;
        return (
          <div
            key={item.id}
            onClick={() => onNavigate?.(item.id)}
            className={cn(
              "flex items-center gap-3 px-3 py-[10px] rounded-sm cursor-pointer text-[14px]",
              "transition-colors duration-[120ms] ease-[cubic-bezier(0.2,0,0,1)]",
              active
                ? "text-nottingham-blue bg-nottingham-blue-5 font-medium"
                : "text-ink-soft font-normal hover:bg-[rgba(16,38,59,0.04)]",
            )}
          >
            <item.Icon className="w-[18px] h-[18px] shrink-0" />
            <span className="flex-1">{item.label}</span>
            {badge !== undefined && (
              <span
                className={cn(
                  "font-mono text-[11px] px-1.5 py-px rounded-pill min-w-4 text-center",
                  active
                    ? "bg-nottingham-blue text-paper"
                    : "bg-portland-stone text-ink-soft",
                )}
              >
                {badge}
              </span>
            )}
          </div>
        );
      })}
    </div>
  );
}
