"use server";

import { revalidatePath } from "next/cache";

import { acceptMembership, declineMembership } from "@/lib/api/users";
import { getAccessToken } from "@/lib/auth";

/**
 * Accepts a team invitation on behalf of the current user.
 * @param teamId - Team GUID to accept
 */
export async function acceptInvitationAction(teamId: string): Promise<void> {
  const token = (await getAccessToken())!;
  await acceptMembership(teamId, token);
  revalidatePath("/invitations");
}

/**
 * Declines a team invitation on behalf of the current user.
 * @param teamId - Team GUID to decline
 */
export async function declineInvitationAction(teamId: string): Promise<void> {
  const token = (await getAccessToken())!;
  await declineMembership(teamId, token);
  revalidatePath("/invitations");
}
