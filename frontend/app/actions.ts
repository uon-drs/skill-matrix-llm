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

/**
 * Initiates Keycloak's registration flow via a server action.
 * The prompt=create authorisation parameter tells Keycloak to open its
 * registration page rather than the login page.
 */
export async function signUpWithKeycloak() {
  await signIn("keycloak", { redirectTo: "/dashboard" }, { prompt: "create" });
}
