import { redirect } from "next/navigation";

import { getSession } from "@/lib/auth";

export default async function DashboardPage() {
  const session = await getSession();
  if (!session) redirect("/");

  return (
    <main className="flex flex-col items-center justify-center min-h-screen bg-paper px-6 text-center">
      <h1 className="text-3xl font-bold tracking-[-0.025em] text-ink mb-3">
        Your project dashboard
      </h1>
      <p className="text-[15px] text-ink-soft">
        Dashboard functionality coming soon.
      </p>
    </main>
  );
}
