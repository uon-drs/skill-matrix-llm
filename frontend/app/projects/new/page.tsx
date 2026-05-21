import { CreateProjectForm } from "./_CreateProjectForm";

/**
 * Page for creating a new project.
 */
export default function NewProjectPage() {
  return (
    <main className="bg-paper px-6 py-10 min-h-[calc(100vh-60px)]">
      <div className="max-w-2xl mx-auto flex flex-col gap-6">
        <h1 className="text-[22px] font-semibold tracking-[-0.02em] text-ink">
          Create a project
        </h1>

        <div className="border border-[var(--border)] rounded-md bg-white p-6">
          <CreateProjectForm />
        </div>
      </div>
    </main>
  );
}
