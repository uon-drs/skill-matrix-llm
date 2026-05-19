// Matchboard root — owns route state, renders TopBar + LeftRail + active screen.

function App() {
  const [route, setRoute] = React.useState("discover");
  const [projectId, setProjectId] = React.useState(null);
  const [researcherId, setResearcherId] = React.useState(null);
  const [toast, setToast] = React.useState(null);
  const [navOpen, setNavOpen] = React.useState(false);

  const navigate = (r) => {
    setRoute(r);
    setProjectId(null);
    setResearcherId(null);
  };

  const openProject = (id) => { setRoute("project"); setProjectId(id); setResearcherId(null); };
  const openResearcher = (id) => { setRoute("researcher"); setResearcherId(id); };

  const showToast = (msg) => {
    setToast(msg);
    setTimeout(() => setToast(null), 3500);
  };

  let screen = null;
  if (route === "discover") {
    screen = <DiscoverScreen openProject={openProject}/>;
  } else if (route === "project") {
    screen = <ProjectDetailScreen
      projectId={projectId}
      openResearcher={openResearcher}
      navigate={navigate}
      onRequestJoin={(id) => showToast("Request sent to project lead")}
    />;
  } else if (route === "researcher" || route === "profile-me") {
    screen = <ResearcherScreen
      researcherId={researcherId || "r-okonkwo"}
      navigate={navigate}
    />;
  } else if (route === "post") {
    screen = <PostProjectScreen
      onSubmit={(title) => { navigate("discover"); showToast(`"${title}" posted. Matching candidates will appear shortly.`); }}
      navigate={navigate}
    />;
  } else if (route === "matches") {
    screen = <MatchesScreen openProject={openProject}/>;
  } else if (route === "my-projects" || route === "saved" || route === "network") {
    screen = (
      <div className="mb-screen" style={{ maxWidth: 900 }}>
        <PageHeader
          eyebrow={route.replace("-", " ")}
          title={{
            "my-projects": "Your posted projects",
            "saved": "Saved researchers & projects",
            "network": "Your collaboration network",
          }[route]}
          description="Placeholder screen — not part of the core five-screen demo."
        />
        <EmptyState
          icon="folder"
          title="Nothing here yet"
          description="This area is a stub. Return to Discover to see the populated screens."
          action={<Button variant="secondary" onClick={() => navigate("discover")}>Back to Discover</Button>}
        />
      </div>
    );
  }

  return (
    <div style={{ minHeight: "100vh", background: "var(--bg-1)" }} data-screen-label={`Matchboard / ${route}`}>
      <TopBar
        route={route}
        onLogo={() => navigate("discover")}
        onPostProject={() => navigate("post")}
        onMenuToggle={() => setNavOpen(true)}
        onNavigate={(r) => { if (r === "profile-me") { setRoute("researcher"); setResearcherId("r-okonkwo"); } else navigate(r); }}
      />
      <div style={{ display: "flex" }}>
        <LeftRail
          route={route === "project" || route === "researcher" ? "discover" : route}
          onNavigate={navigate}
          open={navOpen}
          onClose={() => setNavOpen(false)}
        />
        <main style={{ flex: 1, minWidth: 0 }}>
          {screen}
        </main>
      </div>
      {toast && <Toast message={toast} onDismiss={() => setToast(null)}/>}
    </div>
  );
}

ReactDOM.createRoot(document.getElementById("root")).render(<App />);
