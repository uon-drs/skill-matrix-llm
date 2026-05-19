"use client";

import { ArrowRightIcon } from "@heroicons/react/24/outline";

import { Avatar } from "@/components/core/Avatar";
import { MatchMeter } from "@/components/core/MatchMeter";
import { cn } from "@/lib/utils";

import type { Profile } from "./types";

interface MatchRowProps {
  profile: Profile;
  onClick?: () => void;
  className?: string;
}

/**
 * Compact ranking row used in candidate lists.
 * Displays avatar, name with match-reason summary, a MatchMeter bar, and a
 * right-facing arrow. Background transitions to `--bg-2` on hover.
 * @param profile - Profile data object
 * @param onClick - Click handler
 */
export function MatchRow({ profile, onClick, className }: MatchRowProps) {
  return (
    <div
      onClick={onClick}
      className={cn(
        "grid [grid-template-columns:auto_1fr_auto_auto] gap-[14px] items-center",
        "px-3 py-3 rounded-sm cursor-pointer",
        "bg-transparent hover:bg-portland-stone",
        "transition-colors duration-[120ms] ease-[cubic-bezier(0.2,0,0,1)]",
        className,
      )}
    >
      <Avatar initials={profile.initials} hue={profile.avatarHue} size="md" />

      <div>
        <div className="text-[14px] font-medium text-ink tracking-[-0.005em]">
          {profile.name}
        </div>
        <div className="font-mono text-[11.5px] text-ink-muted mt-[2px]">
          {profile.discipline} · overlap on{" "}
          {profile.matchReasons.slice(0, 2).join(", ")}
        </div>
      </div>

      <MatchMeter pct={profile.matchPct} width={100} />

      <ArrowRightIcon className="w-4 h-4 text-ink-muted shrink-0" />
    </div>
  );
}
