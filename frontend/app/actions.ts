"use server";

import { redirect } from "next/navigation";

import { auth, signIn, signOut } from "@/auth";

/**
 * Initiates the Keycloak OAuth flow via a server action.
 * Using a server action (rather than signIn from next-auth/react) ensures
 * the redirect to Keycloak is issued as an HTTP response, keeping the
 * Next.js router state intact so the back button works correctly.
 */
export async function signInWithKeycloak() {
  await signIn("keycloak", { redirectTo: "/dashboard" });
}

/**
 * Initiates Keycloak's registration flow via a server action.
 * The prompt=create authorisation parameter tells Keycloak to open its
 * registration page rather than the login page.
 */
export async function signUpWithKeycloak() {
  await signIn("keycloak", { redirectTo: "/dashboard" }, { prompt: "create" });
}

/**
 * Signs the user out locally and ends the Keycloak SSO session.
 * next-auth's signOut only clears the local cookie; without also calling
 * Keycloak's end-session endpoint the SSO session stays alive and the user
 * is silently re-authenticated on the next sign-in attempt.
 */
export async function signOutFromKeycloak() {
  const session = await auth();
  const idToken = session?.idToken;

  await signOut({ redirect: false });

  const params = new URLSearchParams({
    post_logout_redirect_uri: `${process.env.AUTH_URL ?? "http://localhost:3000"}/`,
    client_id: process.env.AUTH_KEYCLOAK_ID!,
    ...(idToken ? { id_token_hint: idToken } : {}),
  });

  redirect(
    `${process.env.AUTH_KEYCLOAK_ISSUER}/protocol/openid-connect/logout?${params}`,
  );
}
