import Link from "next/link";
import { redirect } from "next/navigation";

import { LandingTopBar } from "@/components/shared";
import { getSession } from "@/lib/auth";

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
        <div className="flex items-center gap-3">
          <Link
            href="/sign-in"
            className="inline-flex items-center gap-2 font-sans font-medium tracking-[-0.005em] rounded-sm px-[16px] py-[9px] text-[14px] bg-paper text-ink border border-[var(--border-strong)] hover:bg-portland-stone transition-colors duration-[120ms]"
          >
            Sign in
          </Link>
          <Link
            href="/sign-up"
            className="inline-flex items-center gap-2 font-sans font-medium tracking-[-0.005em] rounded-sm px-[16px] py-[9px] text-[14px] bg-nottingham-blue text-paper border border-nottingham-blue hover:bg-[var(--color-primary-deep)] transition-colors duration-[120ms]"
          >
            Sign up
          </Link>
        </div>
      </main>
    </>
  );
}
