import Link from "next/link";
import { redirect } from "next/navigation";

import { signOutFromKeycloak } from "@/app/actions";
import { syncAndFetchCurrentUser } from "@/lib/api/users";
import { getAccessToken, getSession } from "@/lib/auth";

export default async function DashboardPage() {
  const session = await getSession();
  if (!session) redirect("/");

  const token = await getAccessToken();
  const currentUser = token
    ? await syncAndFetchCurrentUser(token).catch(() => null)
    : null;

  return (
    <main className="flex flex-col items-center justify-center min-h-screen bg-paper px-6 text-center">
      <h1 className="text-3xl font-bold tracking-[-0.025em] text-ink mb-3">
        Your project dashboard
      </h1>
      <p className="text-[15px] text-ink-soft mb-8">
        Dashboard functionality coming soon.
      </p>
      <div className="flex items-center gap-3">
        {currentUser && (
          <Link
            href={`/profile/${currentUser.id}`}
            className="inline-flex items-center gap-2 font-sans font-medium tracking-[-0.005em] rounded-sm px-[16px] py-[9px] text-[14px] bg-nottingham-blue text-paper hover:bg-nottingham-blue/90 transition-colors duration-[120ms]"
          >
            My profile
          </Link>
        )}
        <form action={signOutFromKeycloak}>
          <button
            type="submit"
            className="inline-flex items-center gap-2 font-sans font-medium tracking-[-0.005em] rounded-sm px-[16px] py-[9px] text-[14px] bg-paper text-ink border border-[var(--border-strong)] hover:bg-portland-stone transition-colors duration-[120ms] cursor-pointer"
          >
            Sign out
          </button>
        </form>
      </div>
    </main>
  );
}
