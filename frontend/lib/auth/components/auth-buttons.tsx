"use client";

import { signIn, signOut } from "next-auth/react";

import { Button } from "@/components/core";

export function SignInButton() {
  return (
    <Button onClick={() => signIn("keycloak")}>Sign in with Keycloak</Button>
  );
}

export function SignOutButton() {
  return <Button onClick={() => signOut()}>Sign out</Button>;
}
