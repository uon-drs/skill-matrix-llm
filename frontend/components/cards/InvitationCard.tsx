"use client";

import { useState, useTransition } from "react";

import {
  acceptInvitationAction,
  declineInvitationAction,
} from "@/app/invitations/actions";
import { Button } from "@/components/core/Button";
import { StatusPill } from "@/components/core/StatusPill";
import { Toast } from "@/components/core/Toast";
import type { ProjectStatus, UserTeamMembershipDto } from "@/lib/api/types";
import { cn } from "@/lib/utils";

function toStatusPill(projectStatus: ProjectStatus) {
  switch (projectStatus) {
    case "Open":
      return <StatusPill status="open" />;
    case "TeamConfirmed":
      return <StatusPill status="accepted" label="Team confirmed" />;
    case "Closed":
      return <StatusPill status="closed" />;
    default:
      return null;
  }
}

interface InvitationCardProps {
  membership: UserTeamMembershipDto;
}

/**
 * Card displaying a single team invitation or self-request.
 * Invited entries show Accept/Decline buttons; Requested entries show an awaiting-response indicator.
 * @param membership - The membership record to display
 */
export function InvitationCard({ membership }: InvitationCardProps) {
  const [isPendingAccept, startAccept] = useTransition();
  const [isPendingDecline, startDecline] = useTransition();
  const [toast, setToast] = useState<string | null>(null);

  const busy = isPendingAccept || isPendingDecline;

  function handleAccept() {
    setToast("Invitation accepted");
    startAccept(async () => {
      await acceptInvitationAction(membership.teamId);
    });
  }

  function handleDecline() {
    setToast("Invitation declined");
    startDecline(async () => {
      await declineInvitationAction(membership.teamId);
    });
  }

  return (
    <>
      <article
        className={cn(
          "bg-paper border border-[var(--border)] rounded-md p-[22px]",
          "flex flex-col gap-3",
          busy && "opacity-60",
        )}
      >
        <div className="flex items-start justify-between gap-4">
          <div className="min-w-0">
            <h3 className="text-[18px] font-medium tracking-[-0.015em] leading-[1.25] text-ink m-0">
              {membership.projectTitle}
            </h3>
            <p className="text-[13px] text-ink-soft mt-1 m-0">
              Role: <span className="text-ink">{membership.projectRole}</span>
            </p>
          </div>
          <div className="shrink-0 mt-0.5">
            {toStatusPill(membership.projectStatus)}
          </div>
        </div>

        {membership.membershipStatus === "Invited" && (
          <div className="flex gap-2 pt-1">
            <Button size="sm" onClick={handleAccept} disabled={busy}>
              Accept
            </Button>
            <Button
              size="sm"
              variant="destructive"
              onClick={handleDecline}
              disabled={busy}
            >
              Decline
            </Button>
          </div>
        )}

        {membership.membershipStatus === "Requested" && (
          <div className="pt-1">
            <StatusPill status="pending" label="Awaiting response" />
          </div>
        )}
      </article>

      {toast && <Toast message={toast} onDismiss={() => setToast(null)} />}
    </>
  );
}
