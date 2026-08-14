import { redirect } from "next/navigation";

import { fetchMyMemberships, syncAndFetchCurrentUser } from "@/lib/api/users";
import { getAccessToken, getSession } from "@/lib/auth";

import { AppShell } from "../profile/_AppShell";

/**
 * Authenticated layout for the dashboard page — provides TopBar and LeftRail.
 */
export default async function DashboardLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  const session = await getSession();
  if (!session) redirect("/");

  const token = await getAccessToken();
  if (!token) redirect("/");

  const [currentUser, memberships] = await Promise.all([
    syncAndFetchCurrentUser(token),
    fetchMyMemberships(token),
  ]);

  const initials = currentUser.displayName
    .split(" ")
    .map((w) => w[0])
    .join("")
    .slice(0, 2)
    .toUpperCase();

  const pendingInviteCount = memberships.filter(
    (m) =>
      m.membershipStatus === "Invited" || m.membershipStatus === "Requested",
  ).length;

  return (
    <AppShell
      userId={currentUser.id}
      userInitials={initials}
      pendingInviteCount={pendingInviteCount}
    >
      {children}
    </AppShell>
  );
}
