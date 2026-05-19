// Matchboard screens — Discover, Project detail, Researcher profile, Post project, Matches.

const { useState: useStateS } = React;

// ============ Discover ============
function DiscoverScreen({ openProject }) {
  const [filter, setFilter] = useStateS("all");
  const filters = [
    { id: "all", label: "All disciplines" },
    { id: "match", label: "Strong matches" },
    { id: "open", label: "Open" },
    { id: "closing", label: "Closing soon" },
  ];

  const visible = PROJECTS.filter((p) => {
    if (filter === "match") return p.matchPct >= 70;
    if (filter === "closing") return p.deadlineUrgent;
    return true;
  });

  return (
    <div className="mb-screen" style={{ maxWidth: 1100 }}>
      <PageHeader
        eyebrow={`Welcome back, ${ME.name.split(" ")[0]}`}
        title="Projects that match your expertise"
        description={`Ranked against your profile in ${ME.discipline}. Adjust your skills in your profile to broaden or sharpen these results.`}
      />

      {/* Filter row */}
      <div style={{
        display: "flex",
        alignItems: "center",
        gap: 12,
        marginBottom: 20,
        paddingBottom: 16,
        borderBottom: "1px solid var(--border)",
      }}>
        <Icon name="filter" size={16} color="var(--fg-3)" style={{ flexShrink: 0 }}/>
        <div className="mb-chips" style={{ display: "flex", gap: 8, flexWrap: "wrap", flex: 1, minWidth: 0 }}>
          {filters.map((f) => (
            <Chip key={f.id} active={filter === f.id} onClick={() => setFilter(f.id)}>{f.label}</Chip>
          ))}
        </div>
        <div className="mb-desktop-only" style={{
          display: "flex", alignItems: "center", gap: 8,
          fontFamily: "var(--font-mono)", fontSize: 12, color: "var(--fg-3)",
          cursor: "pointer", flexShrink: 0,
        }}>
          <span>Sort: match strength</span>
          <Icon name="chevronDown" size={14}/>
        </div>
      </div>

      {/* Project cards grid */}
      {visible.length > 0 ? (
        <div className="mb-grid-cards">
          {visible.map((p) => (
            <ProjectCard key={p.id} project={p} onClick={() => openProject(p.id)}/>
          ))}
        </div>
      ) : (
        <EmptyState
          icon="search"
          title="No matching projects yet"
          description="Add a method or technique to your project profile to broaden the search."
          action={<Button variant="secondary">Edit profile</Button>}
        />
      )}
    </div>
  );
}

// ============ Project detail ============
function ProjectDetailScreen({ projectId, openResearcher, navigate, onRequestJoin }) {
  const project = PROJECTS.find((p) => p.id === projectId);
  const [requested, setRequested] = useStateS(false);
  if (!project) return null;

  const candidates = project.candidateIds.map((id) => RESEARCHERS.find((r) => r.id === id)).filter(Boolean);

  return (
    <div className="mb-screen" style={{ maxWidth: 1100 }}>
      {/* Back */}
      <button
        onClick={() => navigate("discover")}
        style={{
          display: "inline-flex", alignItems: "center", gap: 8,
          background: "none", border: "none", padding: 0, cursor: "pointer",
          color: "var(--fg-2)", fontSize: 13, marginBottom: 24,
          fontFamily: "var(--font-sans)",
        }}>
        <Icon name="arrowLeft" size={14}/> Back to Discover
      </button>

      {/* Header */}
      <PageHeader
        eyebrow={`${project.discipline} · ${project.code}`}
        title={project.title}
        meta={
          <>
            <StatusPill status={project.deadlineUrgent ? "urgent" : "open"} label={project.deadlineUrgent ? "Closing today" : project.status}/>
            <span>Posted {project.posted}</span>
            <span>Duration {project.duration}</span>
            {project.funded && <span style={{ color: "var(--color-forest-green)" }}>Funded</span>}
            <span>Match {project.matchPct}%</span>
          </>
        }
        action={
          requested ? (
            <Button variant="secondary" size="lg" icon="check" disabled>Request sent</Button>
          ) : (
            <Button variant="primary" size="lg" iconRight="arrowRight" onClick={() => { setRequested(true); onRequestJoin && onRequestJoin(project.id); }}>
              Request to join
            </Button>
          )
        }
      />

      <div className="mb-two-col">
        {/* Left column: description */}
        <div>
          <Eyebrow style={{ marginBottom: 10 }}>About this project</Eyebrow>
          <p style={{ fontSize: 16, color: "var(--fg-1)", lineHeight: 1.6, margin: 0 }}>
            {project.longDescription}
          </p>

          <div style={{ marginTop: 32 }}>
            <Eyebrow style={{ marginBottom: 12 }}>Required skills</Eyebrow>
            <div style={{ display: "flex", flexWrap: "wrap", gap: 6 }}>
              {project.requiredSkills.map((s) => <Tag key={s} variant="skill">{s}</Tag>)}
            </div>
          </div>

          <div style={{ marginTop: 24 }}>
            <Eyebrow style={{ marginBottom: 12 }}>Nice to have</Eyebrow>
            <div style={{ display: "flex", flexWrap: "wrap", gap: 6 }}>
              {project.niceSkills.map((s) => <Tag key={s} variant="neutral">{s}</Tag>)}
            </div>
          </div>

          {/* Candidate ranking */}
          <div style={{ marginTop: 40 }}>
            <div style={{ display: "flex", alignItems: "center", justifyContent: "space-between", marginBottom: 12 }}>
              <Eyebrow>Suggested candidates · {candidates.length}</Eyebrow>
              <span style={{ fontFamily: "var(--font-mono)", fontSize: 11.5, color: "var(--fg-3)" }}>
                Ranked by skill overlap
              </span>
            </div>
            <div style={{ border: "1px solid var(--border)", borderRadius: 6, padding: 8, background: "var(--bg-1)" }}>
              {candidates.sort((a, b) => b.matchPct - a.matchPct).map((r) => (
                <MatchRow key={r.id} researcher={r} onClick={() => openResearcher(r.id)}/>
              ))}
            </div>
          </div>
        </div>

        {/* Right column: lead + at-a-glance */}
        <aside style={{ alignSelf: "start", position: "sticky", top: 84 }}>
          <div style={{ border: "1px solid var(--border)", borderRadius: 6, padding: 20, background: "var(--bg-1)" }}>
            <Eyebrow style={{ marginBottom: 14 }}>Project lead</Eyebrow>
            <div style={{ display: "flex", gap: 12, alignItems: "center", marginBottom: 14 }}>
              <Avatar initials={project.lead.initials} hue={0} size="lg"/>
              <div>
                <div style={{ fontSize: 15, fontWeight: 500, color: "var(--fg-1)", letterSpacing: "-0.005em" }}>
                  {project.lead.name}
                </div>
                <div style={{ fontSize: 12.5, color: "var(--fg-2)", marginTop: 2 }}>
                  {project.discipline}
                </div>
              </div>
            </div>
            <Button variant="secondary" size="sm" icon="message" style={{ width: "100%", justifyContent: "center" }}>
              Send message
            </Button>
          </div>

          <div style={{ height: 16 }}/>

          <div style={{ border: "1px solid var(--border)", borderRadius: 6, padding: 20, background: "var(--bg-1)" }}>
            <Eyebrow style={{ marginBottom: 14 }}>At a glance</Eyebrow>
            <dl style={{ margin: 0, display: "grid", gridTemplateColumns: "auto 1fr", gap: "10px 16px", fontFamily: "var(--font-mono)", fontSize: 12 }}>
              <dt style={{ color: "var(--fg-3)" }}>Code</dt><dd style={{ margin: 0, color: "var(--fg-1)" }}>{project.code}</dd>
              <dt style={{ color: "var(--fg-3)" }}>Posted</dt><dd style={{ margin: 0, color: "var(--fg-1)" }}>{project.posted}</dd>
              <dt style={{ color: "var(--fg-3)" }}>Deadline</dt><dd style={{ margin: 0, color: project.deadlineUrgent ? "var(--color-oxblood)" : "var(--fg-1)" }}>{project.deadline}</dd>
              <dt style={{ color: "var(--fg-3)" }}>Duration</dt><dd style={{ margin: 0, color: "var(--fg-1)" }}>{project.duration}</dd>
              <dt style={{ color: "var(--fg-3)" }}>Funded</dt><dd style={{ margin: 0, color: "var(--fg-1)" }}>{project.funded ? "Yes" : "Pending"}</dd>
              <dt style={{ color: "var(--fg-3)" }}>Candidates</dt><dd style={{ margin: 0, color: "var(--fg-1)" }}>{candidates.length}</dd>
            </dl>
          </div>
        </aside>
      </div>
    </div>
  );
}

// ============ Researcher profile ============
function ResearcherScreen({ researcherId, navigate }) {
  const r = RESEARCHERS.find((x) => x.id === researcherId);
  if (!r) return null;

  return (
    <div className="mb-screen" style={{ maxWidth: 1000 }}>
      <button
        onClick={() => navigate("discover")}
        style={{
          display: "inline-flex", alignItems: "center", gap: 8,
          background: "none", border: "none", padding: 0, cursor: "pointer",
          color: "var(--fg-2)", fontSize: 13, marginBottom: 24,
          fontFamily: "var(--font-sans)",
        }}>
        <Icon name="arrowLeft" size={14}/> Back
      </button>

      <div className="mb-profile-row">
        <Avatar initials={r.initials} hue={r.avatarHue} size="xl"/>
        <div style={{ flex: 1 }}>
          <Eyebrow color="var(--color-primary)" style={{ marginBottom: 8 }}>{r.discipline}</Eyebrow>
          <h1 className="mb-page-title" style={{
            fontFamily: "var(--font-display)",
            fontSize: 36, fontWeight: 500, letterSpacing: "-0.03em", lineHeight: 1.05,
            color: "var(--fg-1)", margin: 0,
          }}>{r.name}</h1>
          <p style={{ fontSize: 16, color: "var(--fg-2)", margin: "8px 0 0" }}>
            {r.title} · {r.department}
          </p>
          <div style={{ display: "flex", gap: 10, marginTop: 18 }}>
            <Button variant="primary" icon="message">Send message</Button>
            <Button variant="secondary" icon="bookmark">Save</Button>
            <Button variant="ghost" icon="ellipsis"></Button>
          </div>
        </div>
        <div className="mb-profile-stats">
          <div><span style={{ color: "var(--fg-1)", fontSize: 20, fontWeight: 500 }}>{r.publications}</span> publications</div>
          <div><span style={{ color: "var(--fg-1)", fontSize: 20, fontWeight: 500 }}>{r.hIndex}</span> h-index</div>
          <div><span style={{ color: "var(--fg-1)", fontSize: 20, fontWeight: 500 }}>{r.openCollaborations}</span> open collabs</div>
        </div>
      </div>

      <Hairline/>

      <div className="mb-two-col-narrow" style={{ marginTop: 32 }}>
        <div>
          <Eyebrow style={{ marginBottom: 12 }}>About</Eyebrow>
          <p style={{ fontSize: 16, color: "var(--fg-1)", lineHeight: 1.6, margin: 0 }}>{r.bio}</p>

          <div style={{ marginTop: 36 }}>
            <Eyebrow style={{ marginBottom: 12 }}>Recent publications</Eyebrow>
            <div style={{ display: "flex", flexDirection: "column" }}>
              {SAMPLE_PUBS.map((pub, i) => (
                <div key={i} style={{
                  padding: "14px 0",
                  borderBottom: i < SAMPLE_PUBS.length - 1 ? "1px solid var(--border)" : "none",
                }}>
                  <div style={{ fontSize: 15, color: "var(--fg-1)", lineHeight: 1.45, letterSpacing: "-0.005em" }}>
                    {pub.title}
                  </div>
                  <div style={{ fontFamily: "var(--font-mono)", fontSize: 11.5, color: "var(--fg-3)", marginTop: 4 }}>
                    {pub.venue} · {pub.year} · {pub.citations} citations
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>

        <aside>
          <Eyebrow style={{ marginBottom: 12 }}>Skills</Eyebrow>
          <div style={{ display: "flex", flexWrap: "wrap", gap: 6 }}>
            {r.skills.map((s) => <Tag key={s} variant="skill">{s}</Tag>)}
          </div>
        </aside>
      </div>
    </div>
  );
}

const SAMPLE_PUBS = [
  { title: "Supramolecular crosslinks for traceable self-healing polymers", venue: "Nature Materials", year: 2024, citations: 142 },
  { title: "Biocompatibility of degradable scaffolds in cardiovascular implants", venue: "Biomaterials", year: 2023, citations: 87 },
  { title: "FTIR signatures of polymer network recovery after mechanical damage", venue: "Macromolecules", year: 2023, citations: 54 },
];

// ============ Post project ============
function PostProjectScreen({ onSubmit, navigate }) {
  const [title, setTitle] = useStateS("");
  const [discipline, setDiscipline] = useStateS("");
  const [description, setDescription] = useStateS("");
  const [skills, setSkills] = useStateS("");

  const handleSubmit = (e) => {
    e.preventDefault();
    onSubmit(title || "New project");
  };

  return (
    <div className="mb-screen" style={{ maxWidth: 760 }}>
      <button
        onClick={() => navigate("discover")}
        style={{
          display: "inline-flex", alignItems: "center", gap: 8,
          background: "none", border: "none", padding: 0, cursor: "pointer",
          color: "var(--fg-2)", fontSize: 13, marginBottom: 24,
          fontFamily: "var(--font-sans)",
        }}>
        <Icon name="arrowLeft" size={14}/> Cancel
      </button>

      <PageHeader
        eyebrow="New project"
        title="Describe your project"
        description="Project descriptions are matched against researcher skill profiles. Specific methods and techniques produce stronger matches than broad themes."
      />

      <form onSubmit={handleSubmit} style={{ display: "flex", flexDirection: "column", gap: 22 }}>
        <TextInput
          label="Project title"
          value={title}
          onChange={(e) => setTitle(e.target.value)}
          placeholder="e.g. Self-healing polymers for biomedical implants"
        />
        <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 16 }}>
          <TextInput
            label="Primary discipline"
            value={discipline}
            onChange={(e) => setDiscipline(e.target.value)}
            placeholder="Materials Science"
          />
          <TextInput
            label="Expected duration"
            placeholder="12 months"
          />
        </div>
        <Textarea
          label="Description"
          rows={6}
          value={description}
          onChange={(e) => setDescription(e.target.value)}
          placeholder="Outline the project, the methods you'll use, and the expertise you're looking for in collaborators."
          help="Aim for 3–5 sentences. The matcher reads this text alongside your skill tags."
        />
        <TextInput
          label="Required skills"
          value={skills}
          onChange={(e) => setSkills(e.target.value)}
          placeholder="tissue engineering, biocompatibility, polymers"
          help="Comma-separated. Add 3–8 specific techniques or methods."
        />

        <Hairline style={{ marginTop: 8 }}/>

        <div style={{ display: "flex", gap: 10, justifyContent: "flex-end" }}>
          <Button variant="ghost" onClick={() => navigate("discover")}>Save as draft</Button>
          <Button variant="primary" type="submit" iconRight="arrowRight">Post project</Button>
        </div>
      </form>
    </div>
  );
}

// ============ Matches inbox ============
function MatchesScreen({ openProject }) {
  const [tab, setTab] = useStateS("incoming");
  const [incoming, setIncoming] = useStateS(INCOMING.map((m) => ({ ...m })));

  const handleAccept = (id) => setIncoming(incoming.map((m) => m.id === id ? { ...m, status: "accepted" } : m));
  const handleDecline = (id) => setIncoming(incoming.map((m) => m.id === id ? { ...m, status: "declined" } : m));

  return (
    <div className="mb-screen" style={{ maxWidth: 980 }}>
      <PageHeader
        eyebrow="Matches"
        title="Requests & invitations"
        description="Incoming requests are projects whose leads believe you're a strong match. Outgoing are projects you've asked to join."
      />

      <div style={{
        display: "flex",
        gap: 24,
        borderBottom: "1px solid var(--border)",
        marginBottom: 16,
      }}>
        {[
          { id: "incoming", label: "Incoming", count: incoming.filter((m) => m.status === "pending").length },
          { id: "outgoing", label: "Outgoing", count: OUTGOING.length },
        ].map((t) => (
          <button
            key={t.id}
            onClick={() => setTab(t.id)}
            style={{
              background: "none", border: "none", padding: "10px 0",
              borderBottom: `2px solid ${tab === t.id ? "var(--color-forest)" : "transparent"}`,
              marginBottom: -1,
              color: tab === t.id ? "var(--fg-1)" : "var(--fg-2)",
              fontSize: 14, fontWeight: 500, cursor: "pointer",
              fontFamily: "var(--font-sans)",
              display: "inline-flex", alignItems: "center", gap: 8,
              letterSpacing: "-0.005em",
            }}
          >
            {t.label}
            <span style={{
              fontFamily: "var(--font-mono)",
              fontSize: 11,
              background: "var(--bg-2)",
              color: "var(--fg-2)",
              padding: "1px 6px",
              borderRadius: 999,
            }}>{t.count}</span>
          </button>
        ))}
      </div>

      {tab === "incoming" ? (
        <div style={{ display: "flex", flexDirection: "column", gap: 12 }}>
          {incoming.map((m) => (
            <div key={m.id} style={{
              padding: 20,
              border: "1px solid var(--border)",
              borderRadius: 6,
              background: "var(--bg-1)",
              opacity: m.status !== "pending" ? 0.55 : 1,
            }}>
              <div style={{ display: "flex", alignItems: "flex-start", justifyContent: "space-between", gap: 16 }}>
                <div style={{ flex: 1, cursor: "pointer" }} onClick={() => openProject(m.from.id)}>
                  <Eyebrow color="var(--color-forest)">{m.from.discipline} · invitation</Eyebrow>
                  <h3 style={{ margin: "6px 0 0", fontSize: 18, fontWeight: 500, color: "var(--fg-1)", letterSpacing: "-0.01em" }}>
                    {m.from.title}
                  </h3>
                  <p style={{ fontSize: 13.5, color: "var(--fg-2)", margin: "6px 0 0", lineHeight: 1.5 }}>
                    From {m.from.lead.name} · {m.from.posted} · {m.from.matchPct}% match against your profile
                  </p>
                </div>
                <div style={{ display: "flex", gap: 8 }}>
                  {m.status === "pending" ? (
                    <>
                      <Button size="sm" variant="primary" icon="check" onClick={(e) => { e.stopPropagation(); handleAccept(m.id); }}>
                        Accept
                      </Button>
                      <Button size="sm" variant="secondary" icon="x" onClick={(e) => { e.stopPropagation(); handleDecline(m.id); }}>
                        Decline
                      </Button>
                    </>
                  ) : (
                    <StatusPill status={m.status === "accepted" ? "accepted" : "closed"} label={m.status === "accepted" ? "Accepted" : "Declined"}/>
                  )}
                </div>
              </div>
            </div>
          ))}
        </div>
      ) : (
        <div style={{ display: "flex", flexDirection: "column", gap: 12 }}>
          {OUTGOING.map((m) => (
            <div key={m.id} style={{
              padding: 20,
              border: "1px solid var(--border)",
              borderRadius: 6,
              background: "var(--bg-1)",
            }}>
              <div style={{ display: "flex", alignItems: "flex-start", justifyContent: "space-between", gap: 16 }}>
                <div style={{ flex: 1, cursor: "pointer" }} onClick={() => openProject(m.to.id)}>
                  <Eyebrow color="var(--color-forest)">{m.to.discipline}</Eyebrow>
                  <h3 style={{ margin: "6px 0 0", fontSize: 18, fontWeight: 500, color: "var(--fg-1)", letterSpacing: "-0.01em" }}>
                    {m.to.title}
                  </h3>
                  <p style={{ fontSize: 13.5, color: "var(--fg-2)", margin: "6px 0 0", lineHeight: 1.5 }}>
                    Requested {m.sent} · lead {m.to.lead.name}
                  </p>
                </div>
                <StatusPill status={m.status === "accepted" ? "accepted" : "pending"} label={m.status === "accepted" ? "Accepted" : "Awaiting response"}/>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

Object.assign(window, {
  DiscoverScreen, ProjectDetailScreen, ResearcherScreen, PostProjectScreen, MatchesScreen,
});
