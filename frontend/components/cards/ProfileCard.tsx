"use client";

import { Avatar } from "@/components/core/Avatar";
import { MatchBadge } from "@/components/core/MatchBadge";
import { Tag } from "@/components/core/Tag";
import { cn } from "@/lib/utils";

import type { Profile } from "./types";

interface ProfileCardProps {
  profile: Profile;
  /** Click handler; ignored when `href` is supplied. */
  onClick?: () => void;
  /** When provided the card renders as an `<a>` element. */
  href?: string;
  /** Compact layout: smaller avatar, fewer skills, no bio. Defaults to `false`. */
  dense?: boolean;
  /** Whether to show the MatchBadge. Defaults to `true`. */
  showMatch?: boolean;
  className?: string;
}

/**
 * Composite card for a staff member profile.
 * Supports a full and a dense layout variant. Displays avatar, name, title,
 * optional MatchBadge, biography (full only), skill tags, and a stats row
 * (publications, h-index, open collaborations). Border transitions to
 * `--border-strong` on hover.
 * @param profile - Profile data object
 * @param onClick - Click handler (used when no `href` is provided)
 * @param href - Renders the card as a navigable `<a>` element
 * @param dense - Compact layout variant
 * @param showMatch - Whether to show the match badge; defaults to `true`
 */
export function ProfileCard({
  profile,
  onClick,
  href,
  dense = false,
  showMatch = true,
  className,
}: ProfileCardProps) {
  const skillLimit = dense ? 3 : 5;
  const overflow = profile.skills.length - skillLimit;

  const classes = cn(
    "bg-paper border border-[var(--border)] rounded-md flex gap-[14px]",
    dense ? "p-[16px]" : "p-[20px]",
    "cursor-pointer transition-colors duration-[120ms] ease-[cubic-bezier(0.2,0,0,1)]",
    "hover:border-[var(--border-strong)] no-underline text-inherit",
    className,
  );

  const content = (
    <>
      <Avatar
        initials={profile.initials}
        hue={profile.avatarHue}
        size={dense ? "md" : "lg"}
      />

      <div className="flex-1 min-w-0">
        {/* Name, title/discipline, match badge */}
        <div className="flex items-start justify-between gap-3">
          <div>
            <h4 className="text-[16px] font-medium tracking-[-0.01em] text-ink m-0">
              {profile.name}
            </h4>
            <p className="text-[13px] text-ink-soft mt-[2px] mb-0">
              {profile.title} · {profile.discipline}
            </p>
          </div>
          {showMatch && <MatchBadge pct={profile.matchPct} />}
        </div>

        {/* Bio (full layout only) */}
        {!dense && (
          <p className="text-[13px] text-ink-soft leading-[1.5] mt-[10px] mb-0">
            {profile.bio}
          </p>
        )}

        {/* Skill tags */}
        <div className="flex flex-wrap gap-[6px] mt-3">
          {profile.skills.slice(0, skillLimit).map((skill) => (
            <Tag key={skill} variant="skill">
              {skill}
            </Tag>
          ))}
          {overflow > 0 && <Tag variant="neutral">+ {overflow}</Tag>}
        </div>

        {/* Stats: publications, h-index, open collaborations */}
        <div className="flex gap-[14px] mt-3 font-mono text-[11.5px] text-ink-muted">
          <span>{profile.publications} pubs</span>
          <span>·</span>
          <span>h-index {profile.hIndex}</span>
          <span>·</span>
          <span>{profile.openCollaborations} open</span>
        </div>
      </div>
    </>
  );

  if (href) {
    return (
      <a href={href} className={classes}>
        {content}
      </a>
    );
  }

  return (
    <article onClick={onClick} className={classes}>
      {content}
    </article>
  );
}
