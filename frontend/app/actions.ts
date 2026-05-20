"use server";

import { signIn } from "@/auth";

/**
 * Initiates the Keycloak OAuth flow via a server action.
 * Using a server action (rather than signIn from next-auth/react) ensures
 * the redirect to Keycloak is issued as an HTTP response, keeping the
 * Next.js router state intact so the back button works correctly.
 */
export async function signInWithKeycloak() {
  await signIn("keycloak", { redirectTo: "/dashboard" });
}
