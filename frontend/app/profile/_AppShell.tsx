"use client";

import { usePathname, useRouter } from "next/navigation";
import { useState, useTransition } from "react";

import { signOutFromKeycloak } from "@/app/actions";
import { LeftRail } from "@/components/shared/LeftRail";
import { TopBar } from "@/components/shared/TopBar";

const ROUTE_MAP: Record<string, string> = {
  "/projects": "my-projects",
  "/invitations": "matches",
};

interface AppShellProps {
  userInitials: string;
  userHue?: 0 | 1 | 2 | 3;
  children: React.ReactNode;
  /** Number of pending invitations to show as a badge on the Invitations nav item. */
  pendingInviteCount?: number;
}

/**
 * Authenticated page shell providing TopBar and LeftRail with mobile drawer state.
 * @param userInitials - Initials shown in the TopBar avatar
 * @param userHue - Avatar gradient variant (0–3)
 */
export function AppShell({
  userInitials,
  userHue = 1,
  children,
  pendingInviteCount,
}: AppShellProps) {
  const pathname = usePathname();
  const router = useRouter();
  const [drawerOpen, setDrawerOpen] = useState(false);
  const [, startTransition] = useTransition();

  const activeRoute = ROUTE_MAP[pathname] ?? undefined;

  function handleNavigate(routeId: string) {
    const destinations: Record<string, string> = {
      "my-projects": "/projects",
      matches: "/invitations",
    };
    const path = destinations[routeId];
    if (path) router.push(path);
  }

  function handleSignOut() {
    startTransition(() => {
      signOutFromKeycloak();
    });
  }

  return (
    <>
      <TopBar
        userInitials={userInitials}
        userHue={userHue}
        onMenuToggle={() => setDrawerOpen(true)}
        onSignOut={handleSignOut}
      />
      <div className="flex">
        <LeftRail
          route={activeRoute}
          onNavigate={handleNavigate}
          open={drawerOpen}
          onClose={() => setDrawerOpen(false)}
          pendingInviteCount={pendingInviteCount}
        />
        <div className="flex-1 min-w-0">{children}</div>
      </div>
    </>
  );
}
