import { InboxIcon } from "@heroicons/react/24/outline";

import { InvitationCard } from "@/components/cards/InvitationCard";
import { EmptyState } from "@/components/shared/EmptyState";
import { PageHeader } from "@/components/shared/PageHeader";
import { fetchMyMemberships } from "@/lib/api/users";
import { getAccessToken } from "@/lib/auth";

/**
 * Invitation inbox — lists pending team invitations and self-requests for the current user.
 */
export default async function InvitationsPage() {
  const token = (await getAccessToken())!;
  const memberships = await fetchMyMemberships(token);

  const pending = memberships.filter(
    (m) =>
      m.membershipStatus === "Invited" || m.membershipStatus === "Requested",
  );

  return (
    <main className="bg-paper px-6 py-10 min-h-[calc(100vh-60px)]">
      <PageHeader
        title="Invitations"
        description="Projects you have been invited to join or have requested to join."
      />

      {pending.length === 0 ? (
        <EmptyState
          icon={InboxIcon}
          title="No pending invitations"
          description="When a project manager invites you to a team, it will appear here."
        />
      ) : (
        <div className="flex flex-col gap-4 max-w-2xl">
          {pending.map((membership) => (
            <InvitationCard key={membership.teamId} membership={membership} />
          ))}
        </div>
      )}
    </main>
  );
}
