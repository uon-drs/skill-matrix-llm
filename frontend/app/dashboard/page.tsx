import { RocketLaunchIcon } from "@heroicons/react/24/outline";
import Link from "next/link";

import { InvitationCard } from "@/components/cards/InvitationCard";
import { ProjectCard } from "@/components/cards/ProjectCard";
import { EmptyState } from "@/components/shared/EmptyState";
import { PageHeader } from "@/components/shared/PageHeader";
import { fetchProjects } from "@/lib/api/projects";
import { fetchMyMemberships, syncAndFetchCurrentUser } from "@/lib/api/users";
import { getAccessToken } from "@/lib/auth";
import { toCardProject, toStatusPill } from "@/lib/mappers";

/**
 * Post-login landing page — surfaces pending invitations and the current
 * user's projects, split into projects they manage and projects they're a
 * team member of.
 */
export default async function DashboardPage() {
  const token = (await getAccessToken())!;

  const [currentUser, memberships, projects] = await Promise.all([
    syncAndFetchCurrentUser(token),
    fetchMyMemberships(token),
    fetchProjects(token),
  ]);

  const pendingInvitations = memberships.filter(
    (m) =>
      m.membershipStatus === "Invited" || m.membershipStatus === "Requested",
  );

  const managingProjects = projects.filter(
    (p) => p.createdByUser.id === currentUser.id,
  );
  const managingProjectIds = new Set(managingProjects.map((p) => p.id));

  const memberOfRows = memberships.filter(
    (m) =>
      m.membershipStatus === "Accepted" && !managingProjectIds.has(m.projectId),
  );

  const isEmpty =
    pendingInvitations.length === 0 &&
    managingProjects.length === 0 &&
    memberOfRows.length === 0;

  return (
    <main className="bg-paper px-6 py-10 min-h-[calc(100vh-60px)]">
      <div className="max-w-3xl mx-auto flex flex-col gap-8">
        <PageHeader
          title="Dashboard"
          description="Your projects and pending invitations, in one place."
        />

        {isEmpty && (
          <EmptyState
            icon={RocketLaunchIcon}
            title="Nothing here yet"
            description="Post a project to start building a team, or wait for an invitation to land in your inbox."
            action={
              <Link
                href="/projects/new"
                className="inline-flex items-center gap-2 font-sans font-medium tracking-[-0.005em] rounded-sm px-[16px] py-[9px] text-[14px] bg-nottingham-blue text-paper hover:bg-nottingham-blue/90 transition-colors duration-[120ms]"
              >
                Post a project
              </Link>
            }
          />
        )}

        {pendingInvitations.length > 0 && (
          <section className="flex flex-col gap-3">
            <h2 className="text-[15px] font-semibold text-ink">
              Pending invitations
            </h2>
            <div className="flex flex-col gap-4">
              {pendingInvitations.map((membership) => (
                <InvitationCard
                  key={membership.teamId}
                  membership={membership}
                />
              ))}
            </div>
          </section>
        )}

        {managingProjects.length > 0 && (
          <section className="flex flex-col gap-3">
            <h2 className="text-[15px] font-semibold text-ink">Managing</h2>
            <div className="flex flex-col gap-3">
              {managingProjects.map((project) => (
                <ProjectCard
                  key={project.id}
                  project={toCardProject(project)}
                  href={`/projects/${project.id}`}
                />
              ))}
            </div>
          </section>
        )}

        {memberOfRows.length > 0 && (
          <section className="flex flex-col gap-3">
            <h2 className="text-[15px] font-semibold text-ink">Member of</h2>
            <div className="flex flex-col divide-y divide-[var(--border)] border border-[var(--border)] rounded-md">
              {memberOfRows.map((membership) => (
                <Link
                  key={membership.teamId}
                  href={`/projects/${membership.projectId}`}
                  className="flex items-center justify-between gap-4 px-4 py-3 no-underline text-inherit hover:bg-portland-stone transition-colors duration-[120ms]"
                >
                  <div className="min-w-0">
                    <p className="text-[14px] font-medium text-ink truncate m-0">
                      {membership.projectTitle}
                    </p>
                    <p className="text-[13px] text-ink-soft m-0">
                      {membership.projectRole}
                    </p>
                  </div>
                  <div className="shrink-0">
                    {toStatusPill(membership.projectStatus)}
                  </div>
                </Link>
              ))}
            </div>
          </section>
        )}
      </div>
    </main>
  );
}
