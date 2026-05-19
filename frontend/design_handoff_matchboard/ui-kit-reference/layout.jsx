// Matchboard layout — TopBar, LeftRail, PageHeader, EmptyState.
// Includes responsive behaviour: below 900px, TopBar collapses to
// hamburger + logo + avatar, and LeftRail becomes a slide-out drawer.

const { useState: useStateLayout } = React;

function TopBar({ onLogo, onPostProject, onNavigate, route, onMenuToggle }) {
  const [search, setSearch] = useStateLayout("");
  const { isMobile } = useViewport();

  return (
    <header style={{
      position: "sticky",
      top: 0,
      zIndex: 10,
      background: "var(--bg-1)",
      borderBottom: "1px solid var(--border)",
      height: 60,
      display: "flex",
      alignItems: "center",
      padding: isMobile ? "0 16px" : "0 28px",
      gap: isMobile ? 12 : 24,
    }}>
      {/* Mobile: hamburger */}
      {isMobile && (
        <button
          aria-label="Open navigation"
          onClick={onMenuToggle}
          style={{
            width: 36, height: 36, borderRadius: 4, border: "1px solid transparent",
            background: "transparent", cursor: "pointer", color: "var(--fg-1)",
            display: "flex", alignItems: "center", justifyContent: "center",
            marginLeft: -6,
          }}
        >
          <Icon name="menu" size={22}/>
        </button>
      )}

      {/* Parent brand — University of Nottingham */}
      <img
        src="../../assets/uon-logo-blue.png"
        alt="University of Nottingham"
        style={{ height: isMobile ? 28 : 34, width: "auto", cursor: "pointer" }}
        onClick={onLogo}
      />

      {/* Divider + product wordmark (desktop only) */}
      {!isMobile && (
        <>
          <span style={{ width: 1, height: 28, background: "var(--border-strong)" }}/>
          <div onClick={onLogo} style={{
            display: "flex", alignItems: "center", gap: 10, cursor: "pointer",
          }}>
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" width="22" height="22">
              <circle cx="9" cy="12" r="4" fill="#10263B"/>
              <circle cx="17" cy="12" r="2.5" fill="#DEB406"/>
            </svg>
            <span style={{
              fontSize: 18, fontWeight: 600, letterSpacing: "-0.025em", color: "var(--fg-1)",
            }}>Matchboard</span>
          </div>

          <div style={{
            fontFamily: "var(--font-sans)",
            fontSize: 13,
            color: "var(--fg-2)",
          }}>Digital Research Service</div>
        </>
      )}

      {/* Mobile: Matchboard wordmark compact */}
      {isMobile && (
        <div onClick={onLogo} style={{ display: "flex", alignItems: "center", gap: 6, cursor: "pointer" }}>
          <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" width="18" height="18">
            <circle cx="9" cy="12" r="4" fill="#10263B"/>
            <circle cx="17" cy="12" r="2.5" fill="#DEB406"/>
          </svg>
          <span style={{
            fontSize: 16, fontWeight: 600, letterSpacing: "-0.025em", color: "var(--fg-1)",
          }}>Matchboard</span>
        </div>
      )}

      {/* Search — desktop only */}
      {!isMobile && (
        <div style={{ flex: 1, maxWidth: 460, marginLeft: 12, position: "relative" }}>
          <span style={{
            position: "absolute", left: 12, top: "50%", transform: "translateY(-50%)",
            color: "var(--fg-3)", display: "flex",
          }}>
            <Icon name="search" size={16}/>
          </span>
          <input
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            placeholder="Search researchers, projects, skills"
            style={{
              width: "100%",
              padding: "8px 12px 8px 36px",
              background: "var(--bg-2)",
              border: "1px solid transparent",
              borderRadius: 6,
              fontFamily: "var(--font-sans)",
              fontSize: 14,
              color: "var(--fg-1)",
              outline: "none",
              boxSizing: "border-box",
            }}
          />
        </div>
      )}

      <div style={{ flex: 1 }}/>

      {/* Post project — full button on desktop, icon-only on mobile */}
      {isMobile ? (
        <button
          aria-label="Post project"
          onClick={onPostProject}
          style={{
            width: 36, height: 36, borderRadius: 4, border: "none",
            background: "var(--color-primary)", color: "var(--color-paper)",
            cursor: "pointer",
            display: "flex", alignItems: "center", justifyContent: "center",
          }}
        >
          <Icon name="plus" size={18}/>
        </button>
      ) : (
        <Button variant="primary" size="md" icon="plus" onClick={onPostProject}>Post project</Button>
      )}

      {!isMobile && (
        <button style={{
          width: 36, height: 36, borderRadius: 4, border: "1px solid transparent",
          background: "transparent", cursor: "pointer", color: "var(--fg-2)",
          display: "flex", alignItems: "center", justifyContent: "center",
        }}
          onMouseEnter={(e) => e.currentTarget.style.background = "rgba(16,38,59,0.06)"}
          onMouseLeave={(e) => e.currentTarget.style.background = "transparent"}
        >
          <Icon name="bell" size={20}/>
        </button>
      )}

      <Avatar initials={ME.initials} hue={1} size="sm" onClick={() => onNavigate && onNavigate("profile-me")}/>
    </header>
  );
}

const NAV_ITEMS = [
  { id: "discover", label: "Discover", icon: "sparkles" },
  { id: "matches", label: "Matches", icon: "inbox", badge: 2 },
  { id: "my-projects", label: "My projects", icon: "beaker" },
  { id: "saved", label: "Saved", icon: "bookmark" },
  { id: "network", label: "Network", icon: "users" },
];

function NavList({ route, onNavigate, includeMobileSearch, onMobileSearch }) {
  return (
    <div style={{ display: "flex", flexDirection: "column", gap: 2 }}>
      {NAV_ITEMS.map((item) => {
        const active = route === item.id;
        return (
          <div
            key={item.id}
            onClick={() => onNavigate(item.id)}
            onMouseEnter={(e) => { if (!active) e.currentTarget.style.background = "rgba(16,38,59,0.04)"; }}
            onMouseLeave={(e) => { if (!active) e.currentTarget.style.background = "transparent"; }}
            style={{
              display: "flex",
              alignItems: "center",
              gap: 12,
              padding: "10px 12px",
              borderRadius: 4,
              cursor: "pointer",
              color: active ? "var(--color-primary)" : "var(--fg-2)",
              background: active ? "var(--color-primary-soft)" : "transparent",
              fontWeight: active ? 500 : 400,
              fontSize: 14,
              transition: "background 120ms cubic-bezier(0.2,0,0,1)",
            }}
          >
            <Icon name={item.icon} size={18}/>
            <span style={{ flex: 1 }}>{item.label}</span>
            {item.badge && (
              <span style={{
                fontFamily: "var(--font-mono)",
                fontSize: 11,
                background: active ? "var(--color-primary)" : "var(--bg-2)",
                color: active ? "var(--color-paper)" : "var(--fg-2)",
                padding: "1px 6px",
                borderRadius: 999,
                minWidth: 16,
                textAlign: "center",
              }}>{item.badge}</span>
            )}
          </div>
        );
      })}
    </div>
  );
}

function LeftRail({ route, onNavigate, open, onClose }) {
  const { isMobile } = useViewport();

  // Close drawer on route change
  React.useEffect(() => { if (isMobile && open) onClose(); }, [route]);

  const railContent = (
    <>
      <NavList route={route} onNavigate={onNavigate}/>

      <div style={{ height: 24 }}/>
      <Hairline style={{ width: "calc(100% - 12px)" }}/>
      <div style={{ height: 16 }}/>

      <Eyebrow style={{ padding: "0 12px", marginBottom: 8 }}>Disciplines</Eyebrow>
      <div style={{ display: "flex", flexDirection: "column", gap: 1 }}>
        {["Materials Science", "Bioengineering", "Computational Biology", "Climate Science", "Ethics"].map((d) => (
          <div key={d} style={{
            padding: "6px 12px",
            fontSize: 13,
            color: "var(--fg-2)",
            cursor: "pointer",
            borderRadius: 4,
          }}
          onMouseEnter={(e) => e.currentTarget.style.background = "rgba(16,38,59,0.04)"}
          onMouseLeave={(e) => e.currentTarget.style.background = "transparent"}
          >{d}</div>
        ))}
      </div>
    </>
  );

  if (isMobile) {
    return (
      <>
        {/* Scrim */}
        {open && (
          <div
            onClick={onClose}
            style={{
              position: "fixed",
              inset: 0,
              background: "rgba(16, 38, 59, 0.4)",
              zIndex: 20,
              animation: "mb-fade 180ms cubic-bezier(0.2,0,0,1)",
            }}
          />
        )}
        {/* Drawer */}
        <nav style={{
          position: "fixed",
          top: 0, left: 0, bottom: 0,
          width: 280,
          maxWidth: "85vw",
          background: "var(--bg-1)",
          borderRight: "1px solid var(--border)",
          padding: "20px 16px",
          zIndex: 30,
          transform: open ? "translateX(0)" : "translateX(-100%)",
          transition: "transform 220ms cubic-bezier(0.2,0,0,1)",
          overflowY: "auto",
          boxShadow: open ? "0 24px 64px -24px rgba(16,38,59,0.32)" : "none",
        }}>
          <div style={{ display: "flex", alignItems: "center", justifyContent: "space-between", marginBottom: 18 }}>
            <img src="../../assets/uon-logo-blue.png" alt="" style={{ height: 28 }}/>
            <button
              aria-label="Close navigation"
              onClick={onClose}
              style={{
                width: 32, height: 32, borderRadius: 4, border: "none",
                background: "transparent", cursor: "pointer", color: "var(--fg-2)",
                display: "flex", alignItems: "center", justifyContent: "center",
              }}
            >
              <Icon name="x" size={18}/>
            </button>
          </div>
          {railContent}
        </nav>
        <style>{`
          @keyframes mb-fade { from { opacity: 0; } to { opacity: 1; } }
        `}</style>
      </>
    );
  }

  return (
    <nav style={{
      width: 220,
      flexShrink: 0,
      padding: "24px 0 24px 16px",
      borderRight: "1px solid var(--border)",
      minHeight: "calc(100vh - 60px)",
      position: "sticky",
      top: 60,
      alignSelf: "flex-start",
    }}>
      {railContent}
    </nav>
  );
}

function PageHeader({ eyebrow, title, description, action, meta }) {
  return (
    <div style={{ marginBottom: 32 }}>
      {eyebrow && <Eyebrow style={{ marginBottom: 10 }}>{eyebrow}</Eyebrow>}
      <div className="mb-page-row">
        <div style={{ flex: 1, minWidth: 0 }}>
          <h1 className="mb-page-title" style={{
            fontFamily: "var(--font-display)",
            fontSize: 36,
            fontWeight: 500,
            letterSpacing: "-0.03em",
            lineHeight: 1.05,
            color: "var(--fg-1)",
            margin: 0,
          }}>{title}</h1>
          {description && (
            <p style={{
              marginTop: 12,
              fontSize: 16,
              color: "var(--fg-2)",
              lineHeight: 1.55,
              maxWidth: 640,
            }}>{description}</p>
          )}
          {meta && (
            <div style={{
              marginTop: 14,
              fontFamily: "var(--font-mono)",
              fontSize: 12,
              color: "var(--fg-3)",
              display: "flex",
              gap: 16,
              flexWrap: "wrap",
            }}>{meta}</div>
          )}
        </div>
        {action && <div>{action}</div>}
      </div>
    </div>
  );
}

function EmptyState({ icon = "search", title, description, action }) {
  return (
    <div style={{
      textAlign: "center",
      padding: "64px 24px",
      border: "1px dashed var(--border-strong)",
      borderRadius: 6,
      color: "var(--fg-2)",
    }}>
      <div style={{ display: "inline-flex", padding: 14, borderRadius: 999, background: "var(--bg-2)", marginBottom: 16 }}>
        <Icon name={icon} size={20} color="var(--fg-3)"/>
      </div>
      <h3 style={{ fontSize: 18, fontWeight: 500, color: "var(--fg-1)", margin: 0, letterSpacing: "-0.01em" }}>{title}</h3>
      {description && (
        <p style={{ fontSize: 14, color: "var(--fg-2)", margin: "8px auto 16px", maxWidth: 380, lineHeight: 1.5 }}>{description}</p>
      )}
      {action}
    </div>
  );
}

Object.assign(window, { TopBar, LeftRail, PageHeader, EmptyState });
