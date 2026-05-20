"use client";

import { XMarkIcon } from "@heroicons/react/24/outline";
import { useSession } from "next-auth/react";
import { useCallback, useEffect, useRef, useState, useTransition } from "react";

import { Tag } from "@/components/core/Tag";
import { searchSkills } from "@/lib/api/skills";
import type {
  SkillCatalogueItem,
  SkillLevel,
  UserProfile,
  UserSkill,
} from "@/lib/api/types";
import { cn } from "@/lib/utils";

import { addSkill, removeSkill, updateSkillLevel } from "./actions";

const LEVELS: SkillLevel[] = ["Basic", "Intermediate", "Pro"];

const levelColour: Record<SkillLevel, "neutral" | "skill" | "discipline"> = {
  Basic: "neutral",
  Intermediate: "skill",
  Pro: "discipline",
};

interface SkillsSectionProps {
  profile: UserProfile;
  isOwnProfile: boolean;
}

/**
 * Displays a user's skills. Own-profile view adds interactive add/remove/update controls.
 * @param profile - The profile whose skills are shown
 * @param isOwnProfile - Whether the viewer is the profile owner
 */
export function SkillsSection({ profile, isOwnProfile }: SkillsSectionProps) {
  const [skills, setSkills] = useState<UserSkill[]>(profile.skills);

  if (!isOwnProfile) {
    if (skills.length === 0) {
      return (
        <p className="text-[14px] text-ink-muted">No skills listed yet.</p>
      );
    }
    return (
      <div className="flex flex-wrap gap-2">
        {skills.map((s) => (
          <Tag key={s.skillId} variant={levelColour[s.level]}>
            {s.skillName} · {s.level}
          </Tag>
        ))}
      </div>
    );
  }

  return (
    <OwnSkillsEditor
      userId={profile.id}
      skills={skills}
      onSkillsChange={setSkills}
    />
  );
}

interface OwnSkillsEditorProps {
  userId: string;
  skills: UserSkill[];
  onSkillsChange: (skills: UserSkill[]) => void;
}

function OwnSkillsEditor({
  userId,
  skills,
  onSkillsChange,
}: OwnSkillsEditorProps) {
  const { data: session } = useSession();
  const [isPending, startTransition] = useTransition();
  const [error, setError] = useState<string | null>(null);

  // Add-skill form state
  const [query, setQuery] = useState("");
  const [results, setResults] = useState<SkillCatalogueItem[]>([]);
  const [selectedSkill, setSelectedSkill] = useState<SkillCatalogueItem | null>(
    null,
  );
  const [addLevel, setAddLevel] = useState<SkillLevel>("Intermediate");
  const [showDropdown, setShowDropdown] = useState(false);
  const debounceRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const dropdownRef = useRef<HTMLDivElement>(null);

  // Debounced skill search
  const handleQueryChange = useCallback(
    (value: string) => {
      setQuery(value);
      setSelectedSkill(null);
      if (debounceRef.current) clearTimeout(debounceRef.current);
      if (!value.trim()) {
        setResults([]);
        setShowDropdown(false);
        return;
      }
      debounceRef.current = setTimeout(async () => {
        const token = session?.accessToken;
        if (!token) return;
        try {
          const items = await searchSkills(value, token);
          // Filter out already-added skills
          const added = new Set(skills.map((s) => s.skillId));
          setResults(items.filter((i) => !added.has(i.id)));
          setShowDropdown(true);
        } catch {
          // silent fail on search errors
        }
      }, 250);
    },
    [session, skills],
  );

  // Close dropdown when clicking outside
  useEffect(() => {
    function handleClick(e: MouseEvent) {
      if (
        dropdownRef.current &&
        !dropdownRef.current.contains(e.target as Node)
      ) {
        setShowDropdown(false);
      }
    }
    document.addEventListener("mousedown", handleClick);
    return () => document.removeEventListener("mousedown", handleClick);
  }, []);

  function handleSelectResult(item: SkillCatalogueItem) {
    setSelectedSkill(item);
    setQuery(item.name);
    setShowDropdown(false);
  }

  function handleAddSkill() {
    if (!selectedSkill) return;
    setError(null);
    startTransition(async () => {
      try {
        const newSkill = await addSkill(userId, selectedSkill.id, addLevel);
        onSkillsChange([...skills, newSkill]);
        setQuery("");
        setSelectedSkill(null);
        setAddLevel("Intermediate");
      } catch {
        setError("Failed to add skill. Please try again.");
      }
    });
  }

  function handleLevelChange(skillId: string, level: SkillLevel) {
    setError(null);
    startTransition(async () => {
      try {
        const updated = await updateSkillLevel(userId, skillId, level);
        onSkillsChange(
          skills.map((s) => (s.skillId === skillId ? updated : s)),
        );
      } catch {
        setError("Failed to update skill level.");
      }
    });
  }

  function handleRemove(skillId: string) {
    setError(null);
    startTransition(async () => {
      try {
        await removeSkill(userId, skillId);
        onSkillsChange(skills.filter((s) => s.skillId !== skillId));
      } catch {
        setError("Failed to remove skill.");
      }
    });
  }

  return (
    <div className="flex flex-col gap-5">
      {/* Current skills list */}
      {skills.length === 0 ? (
        <p className="text-[14px] text-ink-muted">
          No skills added yet. Use the form below to add your first skill.
        </p>
      ) : (
        <ul className="flex flex-col divide-y divide-[var(--border)]">
          {skills.map((skill) => (
            <li
              key={skill.skillId}
              className="flex items-center gap-3 py-2.5 first:pt-0 last:pb-0"
            >
              <span className="flex-1 text-[14px] text-ink font-medium">
                {skill.skillName}
              </span>
              <select
                value={skill.level}
                onChange={(e) =>
                  handleLevelChange(skill.skillId, e.target.value as SkillLevel)
                }
                disabled={isPending}
                className="text-[13px] text-ink border border-[var(--border)] rounded-sm px-2 py-1 bg-paper cursor-pointer hover:border-[var(--border-strong)] transition-colors duration-[120ms] disabled:opacity-50"
              >
                {LEVELS.map((l) => (
                  <option key={l} value={l}>
                    {l}
                  </option>
                ))}
              </select>
              <button
                type="button"
                onClick={() => handleRemove(skill.skillId)}
                disabled={isPending}
                aria-label={`Remove ${skill.skillName}`}
                className="w-7 h-7 flex items-center justify-center rounded-sm text-ink-muted hover:text-jubilee-red hover:bg-jubilee-red-20 transition-colors duration-[120ms] disabled:opacity-50"
              >
                <XMarkIcon className="w-4 h-4" />
              </button>
            </li>
          ))}
        </ul>
      )}

      {/* Add skill form */}
      <div className="border-t border-[var(--border)] pt-4 flex flex-col gap-3">
        <p className="text-[12px] font-medium text-ink-soft uppercase tracking-[0.06em]">
          Add a skill
        </p>
        <div className="flex gap-2 items-start flex-wrap">
          {/* Skill search */}
          <div className="relative flex-1 min-w-[200px]" ref={dropdownRef}>
            <input
              type="text"
              placeholder="Search skills…"
              value={query}
              onChange={(e) => handleQueryChange(e.target.value)}
              onFocus={() => results.length > 0 && setShowDropdown(true)}
              className={cn(
                "w-full text-[14px] text-ink border border-[var(--border)] rounded-sm px-3 py-[9px]",
                "bg-paper placeholder:text-ink-faint",
                "focus:outline-none focus:border-nottingham-blue focus:ring-2 focus:ring-nottingham-blue/20",
                "transition-[border-color,box-shadow] duration-[120ms]",
              )}
            />
            {showDropdown && results.length > 0 && (
              <ul className="absolute z-10 top-full mt-1 left-0 right-0 bg-white border border-[var(--border)] rounded-sm shadow-md max-h-48 overflow-y-auto">
                {results.map((item) => (
                  <li key={item.id}>
                    <button
                      type="button"
                      onMouseDown={(e) => {
                        e.preventDefault();
                        handleSelectResult(item);
                      }}
                      className="w-full text-left px-3 py-2 text-[14px] text-ink hover:bg-portland-stone transition-colors duration-[120ms]"
                    >
                      {item.name}
                    </button>
                  </li>
                ))}
              </ul>
            )}
          </div>

          {/* Level selector */}
          <select
            value={addLevel}
            onChange={(e) => setAddLevel(e.target.value as SkillLevel)}
            className="text-[14px] text-ink border border-[var(--border)] rounded-sm px-3 py-[9px] bg-paper cursor-pointer hover:border-[var(--border-strong)] transition-colors duration-[120ms]"
          >
            {LEVELS.map((l) => (
              <option key={l} value={l}>
                {l}
              </option>
            ))}
          </select>

          {/* Add button */}
          <button
            type="button"
            onClick={handleAddSkill}
            disabled={!selectedSkill || isPending}
            className={cn(
              "px-[16px] py-[9px] text-[14px] font-medium rounded-sm",
              "font-sans tracking-[-0.005em]",
              "bg-nottingham-blue text-paper",
              "hover:bg-nottingham-blue/90 transition-colors duration-[120ms]",
              "disabled:opacity-40 disabled:cursor-not-allowed",
            )}
          >
            {isPending ? "Saving…" : "Add"}
          </button>
        </div>

        {error && <p className="text-[13px] text-jubilee-red">{error}</p>}
      </div>
    </div>
  );
}
