import { redirect } from "next/navigation";

import { signOutFromKeycloak } from "@/app/actions";
import { getSession } from "@/lib/auth";

export default async function DashboardPage() {
  const session = await getSession();
  if (!session) redirect("/");

  return (
    <main className="flex flex-col items-center justify-center min-h-screen bg-paper px-6 text-center">
      <h1 className="text-3xl font-bold tracking-[-0.025em] text-ink mb-3">
        Your project dashboard
      </h1>
      <p className="text-[15px] text-ink-soft mb-8">
        Dashboard functionality coming soon.
      </p>
      <form action={signOutFromKeycloak}>
        <button
          type="submit"
          className="inline-flex items-center gap-2 font-sans font-medium tracking-[-0.005em] rounded-sm px-[16px] py-[9px] text-[14px] bg-paper text-ink border border-[var(--border-strong)] hover:bg-portland-stone transition-colors duration-[120ms] cursor-pointer"
        >
          Sign out
        </button>
      </form>
    </main>
  );
}
