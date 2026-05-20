import Link from "next/link";

export default function SignUpPage() {
  return (
    <main className="flex flex-col items-center justify-center min-h-screen bg-paper px-6 text-center">
      <h1 className="text-3xl font-bold tracking-[-0.025em] text-ink mb-3">
        Sign up
      </h1>
      <p className="text-[15px] text-ink-soft mb-8">
        Sign-up functionality coming soon.
      </p>
      <Link
        href="/"
        className="text-[14px] font-medium text-nottingham-blue hover:underline"
      >
        Back to home
      </Link>
    </main>
  );
}
