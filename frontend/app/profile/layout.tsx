import { redirect } from "next/navigation";

import { syncAndFetchCurrentUser } from "@/lib/api/users";
import { getAccessToken, getSession } from "@/lib/auth";

import { AppShell } from "./_AppShell";

/**
 * Authenticated layout for all profile pages — provides TopBar and LeftRail.
 */
export default async function ProfileLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  const session = await getSession();
  if (!session) redirect("/");

  const token = await getAccessToken();
  if (!token) redirect("/");

  const currentUser = await syncAndFetchCurrentUser(token);

  const initials = currentUser.displayName
    .split(" ")
    .map((w) => w[0])
    .join("")
    .slice(0, 2)
    .toUpperCase();

  return <AppShell userInitials={initials}>{children}</AppShell>;
}
