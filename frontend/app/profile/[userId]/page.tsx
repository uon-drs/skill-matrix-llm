import { Avatar } from "@/components/core/Avatar";
import { Eyebrow } from "@/components/core/Eyebrow";
import { Tag } from "@/components/core/Tag";
import { fetchCurrentUserProfile, fetchUserProfile } from "@/lib/api/users";
import { getAccessToken } from "@/lib/auth";

import { SkillsSection } from "./_SkillsSection";

interface ProfilePageProps {
  params: Promise<{ userId: string }>;
}

/**
 * Profile page for any application user.
 * Shows read-only view for other users; shows editable skills for own profile.
 * Auth and user sync are handled by the parent layout.
 */
export default async function ProfilePage({ params }: ProfilePageProps) {
  const { userId } = await params;
  const token = (await getAccessToken())!;

  const [currentUser, viewedUser] = await Promise.all([
    fetchCurrentUserProfile(token),
    fetchUserProfile(userId, token),
  ]);

  const isOwnProfile = currentUser.id === userId;

  const initials = viewedUser.displayName
    .split(" ")
    .map((w) => w[0])
    .join("")
    .slice(0, 2)
    .toUpperCase();

  return (
    <main className="bg-paper px-6 py-10 min-h-[calc(100vh-60px)]">
      <div className="max-w-2xl mx-auto flex flex-col gap-6">
        {/* Profile header */}
        <div className="border border-[var(--border)] rounded-md bg-white p-6 flex items-start gap-5">
          <Avatar initials={initials} size="xl" hue={1} />
          <div className="flex flex-col gap-1 min-w-0">
            <h1 className="text-[22px] font-semibold tracking-[-0.02em] text-ink leading-tight">
              {viewedUser.displayName}
            </h1>
            <p className="text-[14px] text-ink-soft">{viewedUser.email}</p>
            <div className="mt-2">
              <Tag variant="neutral">{viewedUser.role}</Tag>
            </div>
          </div>
          {isOwnProfile && (
            <div className="ml-auto shrink-0">
              <Eyebrow className="text-rebels-gold">Your profile</Eyebrow>
            </div>
          )}
        </div>

        {/* Skills section */}
        <div className="border border-[var(--border)] rounded-md bg-white p-6 flex flex-col gap-4">
          <h2 className="text-[16px] font-semibold tracking-[-0.015em] text-ink">
            Skills
          </h2>
          <SkillsSection profile={viewedUser} isOwnProfile={isOwnProfile} />
        </div>
      </div>
    </main>
  );
}
