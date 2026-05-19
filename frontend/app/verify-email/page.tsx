/**
 * Email verification page.
 *
 * Reads the `token` search param, POSTs it to the backend verify-email endpoint,
 * and renders a success or error message to the user.
 */
export default async function VerifyEmailPage({
  searchParams,
}: {
  searchParams: Promise<{ token?: string }>;
}) {
  const { token } = await searchParams;

  if (!token) {
    return (
      <main
        style={{
          fontFamily: "sans-serif",
          maxWidth: 640,
          margin: "4rem auto",
          padding: "0 1rem",
        }}
      >
        <h1>Email Verification</h1>
        <p style={{ color: "red" }}>No verification token provided.</p>
      </main>
    );
  }

  let success = false;
  let errorMessage =
    "An unexpected error occurred. Please try again or contact your administrator.";

  try {
    const apiBase = process.env.NEXT_PUBLIC_API_BASE_URL ?? "";
    const response = await fetch(`${apiBase}/api/users/verify-email`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ token }),
      cache: "no-store",
    });

    if (response.ok) {
      success = true;
    } else if (response.status === 404) {
      errorMessage =
        "This verification link is invalid or has already been used.";
    } else if (response.status === 400) {
      errorMessage =
        "This verification link has expired. Please contact your administrator to request a new one.";
    }
  } catch {
    errorMessage =
      "Could not reach the server. Please check your connection and try again.";
  }

  return (
    <main
      style={{
        fontFamily: "sans-serif",
        maxWidth: 640,
        margin: "4rem auto",
        padding: "0 1rem",
      }}
    >
      <h1>Email Verification</h1>
      {success ? (
        <p style={{ color: "green" }}>
          Your email address has been verified successfully.
        </p>
      ) : (
        <p style={{ color: "red" }}>{errorMessage}</p>
      )}
    </main>
  );
}
