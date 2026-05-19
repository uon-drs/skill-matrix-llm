"use client";

import { Avatar } from "@/components/core/Avatar";
import { Eyebrow } from "@/components/core/Eyebrow";
import { MatchBadge } from "@/components/core/MatchBadge";
import { Tag } from "@/components/core/Tag";
import { cn } from "@/lib/utils";

import type { Project } from "./types";

interface ProjectCardProps {
  project: Project;
  /** Click handler; ignored when `href` is supplied. */
  onClick?: () => void;
  /** When provided the card renders as an `<a>` element. */
  href?: string;
  className?: string;
}

const baseClasses =
  "bg-paper border border-[var(--border)] rounded-md p-[22px] flex flex-col gap-[14px] " +
  "cursor-pointer transition-colors duration-[120ms] ease-[cubic-bezier(0.2,0,0,1)] " +
  "hover:border-[var(--border-strong)] no-underline text-inherit";

/**
 * Composite card for a research project.
 * Displays discipline eyebrow, match badge, title, description, required-skill
 * tags, and a footer row with lead avatar, posted date, funding status, and
 * duration. Border transitions to `--border-strong` on hover.
 * @param project - Project data object
 * @param onClick - Click handler (used when no `href` is provided)
 * @param href - Renders the card as a navigable `<a>` element
 */
export function ProjectCard({
  project,
  onClick,
  href,
  className,
}: ProjectCardProps) {
  const classes = cn(baseClasses, className);

  const content = (
    <>
      {/* Eyebrow row: discipline + status, and match badge */}
      <div className="flex items-center justify-between gap-2">
        <Eyebrow
          style={{
            color: project.deadlineUrgent
              ? "var(--color-oxblood)"
              : "var(--color-forest)",
          }}
        >
          {project.discipline} ·{" "}
          {project.deadlineUrgent ? "Closing today" : project.status}
        </Eyebrow>
        <MatchBadge pct={project.matchPct} />
      </div>

      <h3 className="text-[20px] font-medium tracking-[-0.015em] leading-[1.25] text-ink m-0">
        {project.title}
      </h3>

      <p className="text-[14px] text-ink-soft leading-[1.5] m-0">
        {project.description}
      </p>

      <div className="flex flex-wrap gap-[6px]">
        {project.requiredSkills.map((skill) => (
          <Tag key={skill} variant="skill">
            {skill}
          </Tag>
        ))}
      </div>

      {/* Footer: lead avatar, name, posted date, funded flag, duration */}
      <div className="flex items-center gap-[14px] mt-1 pt-[14px] border-t border-[var(--border)] font-mono text-[11.5px] text-ink-muted">
        <Avatar
          initials={project.lead.initials}
          hue={project.lead.avatarHue ?? 0}
          size="sm"
          style={{ width: 22, height: 22, fontSize: "9px" }}
        />
        <span className="text-ink-soft">{project.lead.name}</span>
        <span>·</span>
        <span>{project.posted}</span>
        <span className="ml-auto">
          {project.funded && <span className="text-forest-green">Funded</span>}
          {project.funded && " · "}
          {project.duration}
        </span>
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
