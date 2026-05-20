"use client";

import { useState } from "react";

import { LeftRail } from "@/components/shared/LeftRail";
import { TopBar } from "@/components/shared/TopBar";

interface AppShellProps {
  userInitials: string;
  userHue?: 0 | 1 | 2 | 3;
  children: React.ReactNode;
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
}: AppShellProps) {
  const [drawerOpen, setDrawerOpen] = useState(false);

  return (
    <>
      <TopBar
        userInitials={userInitials}
        userHue={userHue}
        onMenuToggle={() => setDrawerOpen(true)}
      />
      <div className="flex">
        <LeftRail open={drawerOpen} onClose={() => setDrawerOpen(false)} />
        <div className="flex-1 min-w-0">{children}</div>
      </div>
    </>
  );
}
