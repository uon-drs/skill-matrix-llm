"use client";

import { useState, useTransition } from "react";

import { Button } from "@/components/core/Button";
import { Toast } from "@/components/core/Toast";
import type { Team } from "@/lib/api/types";

import { confirmTeamAction, rejectTeamAction } from "./actions";

interface TeamCardProps {
  projectId: string;
  team: Team;
  /** Whether the current viewer may confirm/reject this team. */
  canManage: boolean;
}

/**
 * Displays a single proposed team with its members, and confirm/reject
 * controls for the project's creator while the team is Proposed.
 * @param projectId - Project the team belongs to
 * @param team - The team to display
 * @param canManage - Whether the current viewer may confirm/reject this team
 */
export function TeamCard({ projectId, team, canManage }: TeamCardProps) {
  const [isPendingConfirm, startConfirm] = useTransition();
  const [isPendingReject, startReject] = useTransition();
  const [error, setError] = useState<string | null>(null);
  const [toast, setToast] = useState<string | null>(null);

  const busy = isPendingConfirm || isPendingReject;

  function handleConfirm() {
    setError(null);
    startConfirm(async () => {
      const result = await confirmTeamAction(projectId, team.id);
      if (result.error) setError(result.error);
      else setToast("Team confirmed");
    });
  }

  function handleReject() {
    setError(null);
    startReject(async () => {
      const result = await rejectTeamAction(projectId, team.id);
      if (result.error) setError(result.error);
      else setToast("Team rejected");
    });
  }

  return (
    <>
      <div className="border border-[var(--border)] rounded-sm p-4 flex flex-col gap-2">
        <div className="flex items-center justify-between">
          <span className="text-[13px] font-medium text-ink">Team</span>
          <span className="text-[12px] text-ink-soft">{team.status}</span>
        </div>
        {team.members.length > 0 ? (
          <ul className="flex flex-col gap-1">
            {team.members.map((member) => (
              <li key={member.id} className="text-[13px] text-ink-soft">
                {member.user.displayName}
                {member.projectRole && (
                  <span className="text-ink-muted">
                    {" "}
                    — {member.projectRole}
                  </span>
                )}
              </li>
            ))}
          </ul>
        ) : (
          <p className="text-[13px] text-ink-muted">No members yet.</p>
        )}

        {canManage && team.status === "Proposed" && (
          <div className="flex gap-2 pt-1">
            <Button size="sm" onClick={handleConfirm} disabled={busy}>
              Confirm
            </Button>
            <Button
              size="sm"
              variant="destructive"
              onClick={handleReject}
              disabled={busy}
            >
              Reject
            </Button>
          </div>
        )}

        {error && <p className="text-[13px] text-jubilee-red">{error}</p>}
      </div>

      {toast && <Toast message={toast} onDismiss={() => setToast(null)} />}
    </>
  );
}
