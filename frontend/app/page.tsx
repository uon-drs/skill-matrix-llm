import { redirect } from "next/navigation";

import { LandingTopBar } from "@/components/shared";
import { getSession } from "@/lib/auth";

import { LandingCTA } from "./_LandingCTA";

export default async function HomePage() {
  const session = await getSession();
  if (session) redirect("/dashboard");

  return (
    <>
      <LandingTopBar />
      <main className="flex flex-col items-center justify-center min-h-[calc(100vh-60px)] bg-paper px-6 text-center">
        <h1 className="text-5xl font-bold tracking-[-0.03em] text-ink mb-4">
          Welcome to Matchboard
        </h1>
        <p className="text-[17px] text-ink-soft max-w-[520px] mb-10">
          The Digital Research Service skills-matching platform. Connect
          researchers with the expertise your project needs.
        </p>
        <LandingCTA />
      </main>
    </>
  );
}
