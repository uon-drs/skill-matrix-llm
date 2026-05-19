// Card components: ProjectCard, ResearcherCard, MatchRow.

function ProjectCard({ project, onClick }) {
  const urgent = project.deadlineUrgent;
  return (
    <article
      onClick={onClick}
      onMouseEnter={(e) => e.currentTarget.style.borderColor = "var(--border-strong)"}
      onMouseLeave={(e) => e.currentTarget.style.borderColor = "var(--border)"}
      style={{
        background: "var(--bg-1)",
        border: "1px solid var(--border)",
        borderRadius: 6,
        padding: 22,
        cursor: "pointer",
        transition: "border-color 120ms cubic-bezier(0.2,0,0,1)",
        display: "flex",
        flexDirection: "column",
        gap: 14,
      }}
    >
      {/* Eyebrow row */}
      <div style={{ display: "flex", alignItems: "center", justifyContent: "space-between", gap: 8 }}>
        <Eyebrow color={urgent ? "var(--color-oxblood)" : "var(--color-forest)"}>
          {project.discipline} · {urgent ? "Closing today" : project.status}
        </Eyebrow>
        <MatchBadge pct={project.matchPct}/>
      </div>

      {/* Title */}
      <h3 style={{
        fontSize: 20,
        fontWeight: 500,
        letterSpacing: "-0.015em",
        lineHeight: 1.25,
        color: "var(--fg-1)",
        margin: 0,
      }}>{project.title}</h3>

      {/* Description */}
      <p style={{
        fontSize: 14,
        color: "var(--fg-2)",
        lineHeight: 1.5,
        margin: 0,
      }}>{project.description}</p>

      {/* Required skills */}
      <div style={{ display: "flex", flexWrap: "wrap", gap: 6 }}>
        {project.requiredSkills.map((s) => (
          <Tag key={s} variant="skill">{s}</Tag>
        ))}
      </div>

      {/* Meta footer */}
      <div style={{
        display: "flex",
        alignItems: "center",
        gap: 14,
        marginTop: 4,
        paddingTop: 14,
        borderTop: "1px solid var(--border)",
        fontFamily: "var(--font-mono)",
        fontSize: 11.5,
        color: "var(--fg-3)",
      }}>
        <Avatar initials={project.lead.initials} hue={1} size="sm" style={{ width: 22, height: 22, fontSize: 9 }}/>
        <span style={{ color: "var(--fg-2)" }}>{project.lead.name}</span>
        <span>·</span>
        <span>{project.posted}</span>
        <span style={{ marginLeft: "auto" }}>
          {project.funded && <span style={{ color: "var(--color-forest-green)" }}>Funded</span>}
          {project.funded && " · "}
          {project.duration}
        </span>
      </div>
    </article>
  );
}

function ResearcherCard({ researcher, onClick, dense, showMatch = true }) {
  return (
    <article
      onClick={onClick}
      onMouseEnter={(e) => e.currentTarget.style.borderColor = "var(--border-strong)"}
      onMouseLeave={(e) => e.currentTarget.style.borderColor = "var(--border)"}
      style={{
        background: "var(--bg-1)",
        border: "1px solid var(--border)",
        borderRadius: 6,
        padding: dense ? 16 : 20,
        cursor: "pointer",
        transition: "border-color 120ms cubic-bezier(0.2,0,0,1)",
        display: "flex",
        gap: 14,
      }}
    >
      <Avatar initials={researcher.initials} hue={researcher.avatarHue} size={dense ? "md" : "lg"}/>
      <div style={{ flex: 1, minWidth: 0 }}>
        <div style={{ display: "flex", alignItems: "flex-start", justifyContent: "space-between", gap: 12 }}>
          <div>
            <h4 style={{
              fontSize: 16,
              fontWeight: 500,
              letterSpacing: "-0.01em",
              color: "var(--fg-1)",
              margin: 0,
            }}>{researcher.name}</h4>
            <p style={{
              fontSize: 13,
              color: "var(--fg-2)",
              margin: "2px 0 0",
            }}>{researcher.title} · {researcher.discipline}</p>
          </div>
          {showMatch && <MatchBadge pct={researcher.matchPct}/>}
        </div>

        {!dense && (
          <p style={{
            fontSize: 13,
            color: "var(--fg-2)",
            lineHeight: 1.5,
            margin: "10px 0 0",
          }}>{researcher.bio}</p>
        )}

        <div style={{ display: "flex", flexWrap: "wrap", gap: 6, marginTop: 12 }}>
          {researcher.skills.slice(0, dense ? 3 : 5).map((s) => (
            <Tag key={s} variant="skill">{s}</Tag>
          ))}
          {researcher.skills.length > (dense ? 3 : 5) && (
            <Tag variant="neutral">+ {researcher.skills.length - (dense ? 3 : 5)}</Tag>
          )}
        </div>

        <div style={{
          marginTop: 12,
          fontFamily: "var(--font-mono)",
          fontSize: 11.5,
          color: "var(--fg-3)",
          display: "flex",
          gap: 14,
        }}>
          <span>{researcher.publications} pubs</span>
          <span>·</span>
          <span>h-index {researcher.hIndex}</span>
          <span>·</span>
          <span>{researcher.openCollaborations} open</span>
        </div>
      </div>
    </article>
  );
}

// MatchRow — compact row for candidate ranking lists
function MatchRow({ researcher, onClick }) {
  return (
    <div
      onClick={onClick}
      onMouseEnter={(e) => e.currentTarget.style.background = "var(--bg-2)"}
      onMouseLeave={(e) => e.currentTarget.style.background = "transparent"}
      style={{
        display: "grid",
        gridTemplateColumns: "auto 1fr auto auto",
        gap: 14,
        alignItems: "center",
        padding: "12px 12px",
        borderRadius: 4,
        cursor: "pointer",
        transition: "background 120ms",
      }}
    >
      <Avatar initials={researcher.initials} hue={researcher.avatarHue} size="md"/>
      <div>
        <div style={{ fontSize: 14, fontWeight: 500, color: "var(--fg-1)", letterSpacing: "-0.005em" }}>
          {researcher.name}
        </div>
        <div style={{ fontFamily: "var(--font-mono)", fontSize: 11.5, color: "var(--fg-3)", marginTop: 2 }}>
          {researcher.discipline} · overlap on {researcher.matchReasons.slice(0, 2).join(", ")}
        </div>
      </div>
      <MatchMeter pct={researcher.matchPct} width={100}/>
      <Icon name="arrowRight" size={16} color="var(--fg-3)"/>
    </div>
  );
}

Object.assign(window, { ProjectCard, ResearcherCard, MatchRow });
