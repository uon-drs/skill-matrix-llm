// Matchboard primitives. All visual decisions reference colors_and_type.css.

const { useState } = React;

// ---------- Icon ----------
// Inline Heroicons (outline). Subset used across the kit.
const ICONS = {
  search: <path strokeLinecap="round" strokeLinejoin="round" d="m21 21-5.197-5.197m0 0A7.5 7.5 0 1 0 5.196 5.196a7.5 7.5 0 0 0 10.607 10.607Z"/>,
  bell: <path strokeLinecap="round" strokeLinejoin="round" d="M14.857 17.082a23.848 23.848 0 0 0 5.454-1.31A8.967 8.967 0 0 1 18 9.75V9A6 6 0 0 0 6 9v.75a8.967 8.967 0 0 1-2.312 6.022c1.733.64 3.56 1.085 5.455 1.31m5.714 0a24.255 24.255 0 0 1-5.714 0m5.714 0a3 3 0 1 1-5.714 0"/>,
  sparkles: <path strokeLinecap="round" strokeLinejoin="round" d="M9.813 15.904 9 18.75l-.813-2.846a4.5 4.5 0 0 0-3.09-3.09L2.25 12l2.846-.813a4.5 4.5 0 0 0 3.09-3.09L9 5.25l.813 2.846a4.5 4.5 0 0 0 3.09 3.09L15.75 12l-2.846.813a4.5 4.5 0 0 0-3.09 3.09ZM18.259 8.715 18 9.75l-.259-1.035a3.375 3.375 0 0 0-2.455-2.456L14.25 6l1.036-.259a3.375 3.375 0 0 0 2.455-2.456L18 2.25l.259 1.035a3.375 3.375 0 0 0 2.456 2.456L21.75 6l-1.035.259a3.375 3.375 0 0 0-2.456 2.456Z"/>,
  message: <path strokeLinecap="round" strokeLinejoin="round" d="M2.25 12.76c0 1.6 1.123 2.994 2.707 3.227 1.068.157 2.148.279 3.238.364.466.037.893.281 1.153.671L12 21l2.652-3.978c.26-.39.687-.634 1.153-.67 1.09-.086 2.17-.208 3.238-.365 1.584-.233 2.707-1.626 2.707-3.228V6.741c0-1.602-1.123-2.995-2.707-3.228A48.394 48.394 0 0 0 12 3c-2.392 0-4.744.175-7.043.513C3.373 3.746 2.25 5.14 2.25 6.741v6.018Z"/>,
  users: <path strokeLinecap="round" strokeLinejoin="round" d="M18 18.72a9.094 9.094 0 0 0 3.741-.479 3 3 0 0 0-4.682-2.72m.94 3.198.001.031c0 .225-.012.447-.037.666A11.944 11.944 0 0 1 12 21c-2.17 0-4.207-.576-5.963-1.584A6.062 6.062 0 0 1 6 18.719m12 0a5.971 5.971 0 0 0-.941-3.197m0 0A5.995 5.995 0 0 0 12 12.75a5.995 5.995 0 0 0-5.058 2.772m0 0a3 3 0 0 0-4.681 2.72 8.986 8.986 0 0 0 3.74.477m.94-3.197a5.971 5.971 0 0 0-.94 3.197M15 6.75a3 3 0 1 1-6 0 3 3 0 0 1 6 0Zm6 3a2.25 2.25 0 1 1-4.5 0 2.25 2.25 0 0 1 4.5 0Zm-13.5 0a2.25 2.25 0 1 1-4.5 0 2.25 2.25 0 0 1 4.5 0Z"/>,
  inbox: <path strokeLinecap="round" strokeLinejoin="round" d="M2.25 13.5h3.86a2.25 2.25 0 0 1 2.012 1.244l.256.512a2.25 2.25 0 0 0 2.013 1.244h3.218a2.25 2.25 0 0 0 2.013-1.244l.256-.512a2.25 2.25 0 0 1 2.013-1.244h3.859m-19.5.338V18a2.25 2.25 0 0 0 2.25 2.25h15A2.25 2.25 0 0 0 21.75 18v-4.162c0-.224-.034-.447-.1-.661L19.24 5.338a2.25 2.25 0 0 0-2.15-1.588H6.911a2.25 2.25 0 0 0-2.15 1.588L2.35 13.177a2.25 2.25 0 0 0-.1.661Z"/>,
  plus: <path strokeLinecap="round" strokeLinejoin="round" d="M12 4.5v15m7.5-7.5h-15"/>,
  arrowRight: <path strokeLinecap="round" strokeLinejoin="round" d="M13.5 4.5 21 12m0 0-7.5 7.5M21 12H3"/>,
  arrowLeft: <path strokeLinecap="round" strokeLinejoin="round" d="M10.5 19.5 3 12m0 0 7.5-7.5M3 12h18"/>,
  check: <path strokeLinecap="round" strokeLinejoin="round" d="m4.5 12.75 6 6 9-13.5"/>,
  x: <path strokeLinecap="round" strokeLinejoin="round" d="M6 18 18 6M6 6l12 12"/>,
  beaker: <path strokeLinecap="round" strokeLinejoin="round" d="M9.75 3.104v5.714a2.25 2.25 0 0 1-.659 1.591L5 14.5M9.75 3.104c-.251.023-.501.05-.75.082m.75-.082a24.301 24.301 0 0 1 4.5 0m0 0v5.714c0 .597.237 1.17.659 1.591L19.8 15.3M14.25 3.104c.251.023.501.05.75.082M19.8 15.3l-1.57.393A9.065 9.065 0 0 1 12 15a9.065 9.065 0 0 0-6.23-.693L5 14.5m14.8.8 1.402 1.402c1.232 1.232.65 3.378-1.067 3.711A48.309 48.309 0 0 1 12 21c-2.773 0-5.491-.235-8.135-.687-1.718-.333-2.3-2.479-1.067-3.711L5 14.5"/>,
  bookmark: <path strokeLinecap="round" strokeLinejoin="round" d="M17.593 3.322c1.1.128 1.907 1.077 1.907 2.185V21L12 17.25 4.5 21V5.507c0-1.108.806-2.057 1.907-2.185a48.507 48.507 0 0 1 11.186 0Z"/>,
  ellipsis: <path strokeLinecap="round" strokeLinejoin="round" d="M6.75 12a.75.75 0 1 1-1.5 0 .75.75 0 0 1 1.5 0ZM12.75 12a.75.75 0 1 1-1.5 0 .75.75 0 0 1 1.5 0ZM18.75 12a.75.75 0 1 1-1.5 0 .75.75 0 0 1 1.5 0Z"/>,
  menu: <path strokeLinecap="round" strokeLinejoin="round" d="M3.75 6.75h16.5M3.75 12h16.5m-16.5 5.25h16.5"/>,
  filter: <path strokeLinecap="round" strokeLinejoin="round" d="M12 3c2.755 0 5.455.232 8.083.678.533.09.917.556.917 1.096v1.044a2.25 2.25 0 0 1-.659 1.591l-5.432 5.432a2.25 2.25 0 0 0-.659 1.591v2.927a2.25 2.25 0 0 1-1.244 2.013L9.75 21v-6.568a2.25 2.25 0 0 0-.659-1.591L3.659 7.409A2.25 2.25 0 0 1 3 5.818V4.774c0-.54.384-1.006.917-1.096A48.32 48.32 0 0 1 12 3Z"/>,
  chevronDown: <path strokeLinecap="round" strokeLinejoin="round" d="m19.5 8.25-7.5 7.5-7.5-7.5"/>,
};

function Icon({ name, size = 20, color, style, ...rest }) {
  return (
    <svg
      xmlns="http://www.w3.org/2000/svg"
      fill="none"
      viewBox="0 0 24 24"
      strokeWidth="1.5"
      stroke={color || "currentColor"}
      width={size}
      height={size}
      style={{ display: "inline-block", flexShrink: 0, ...style }}
      {...rest}
    >
      {ICONS[name]}
    </svg>
  );
}

// ---------- Eyebrow ----------
function Eyebrow({ children, color, style }) {
  return (
    <div style={{
      fontSize: 11,
      letterSpacing: "0.14em",
      textTransform: "uppercase",
      fontWeight: 500,
      color: color || "var(--fg-3)",
      ...style,
    }}>
      {children}
    </div>
  );
}

// ---------- Hairline ----------
function Hairline({ vertical, color = "var(--border)", style }) {
  return (
    <div style={{
      background: color,
      ...(vertical ? { width: 1, alignSelf: "stretch" } : { height: 1, width: "100%" }),
      ...style,
    }}/>
  );
}

// ---------- Button ----------
const buttonVariants = {
  primary: { background: "var(--color-forest)", color: "var(--color-paper)", border: "1px solid var(--color-forest)" },
  secondary: { background: "var(--color-paper)", color: "var(--color-ink)", border: "1px solid var(--border-strong)" },
  ghost: { background: "transparent", color: "var(--color-ink)", border: "1px solid transparent" },
  destructive: { background: "var(--color-paper)", color: "var(--color-jubilee-red)", border: "1px solid rgba(185,28,46,0.4)" },
};

function Button({ variant = "primary", size = "md", icon, iconRight, children, onClick, type, disabled, style }) {
  const [hover, setHover] = useState(false);
  const [press, setPress] = useState(false);
  const base = buttonVariants[variant];
  const hoverBg = {
    primary: "var(--color-forest-deep)",
    secondary: "var(--bg-2)",
    ghost: "rgba(16,38,59,0.06)",
    destructive: "var(--color-oxblood-tint)",
  }[variant];

  return (
    <button
      type={type || "button"}
      disabled={disabled}
      onMouseEnter={() => setHover(true)}
      onMouseLeave={() => { setHover(false); setPress(false); }}
      onMouseDown={() => setPress(true)}
      onMouseUp={() => setPress(false)}
      onClick={onClick}
      style={{
        ...base,
        background: hover ? hoverBg : base.background,
        display: "inline-flex",
        alignItems: "center",
        gap: 8,
        padding: size === "sm" ? "5px 10px" : size === "lg" ? "12px 20px" : "9px 16px",
        fontFamily: "var(--font-sans)",
        fontSize: size === "sm" ? 12 : size === "lg" ? 15 : 14,
        fontWeight: 500,
        letterSpacing: "-0.005em",
        borderRadius: 4,
        cursor: disabled ? "not-allowed" : "pointer",
        opacity: disabled ? 0.5 : 1,
        transform: press ? "scale(0.98)" : "scale(1)",
        transition: "background 120ms cubic-bezier(0.2,0,0,1), transform 80ms cubic-bezier(0.2,0,0,1), border-color 120ms",
        ...style,
      }}>
      {icon && <Icon name={icon} size={size === "sm" ? 14 : 16}/>}
      {children}
      {iconRight && <Icon name={iconRight} size={size === "sm" ? 14 : 16}/>}
    </button>
  );
}

// ---------- Tag ----------
function Tag({ variant = "skill", children, style }) {
  const variants = {
    skill: { background: "var(--color-rebels-gold-20)", color: "var(--color-nottingham-blue)" },
    discipline: { background: "var(--color-nottingham-blue-20)", color: "var(--color-nottingham-blue)" },
    neutral: { background: "var(--bg-2)", color: "var(--fg-2)" },
  };
  return (
    <span style={{
      ...variants[variant],
      display: "inline-flex",
      alignItems: "center",
      gap: 6,
      padding: "3px 9px",
      borderRadius: 2,
      fontSize: 12,
      fontFamily: "var(--font-mono)",
      ...style,
    }}>{children}</span>
  );
}

// ---------- Chip (filter) ----------
function Chip({ active, onClick, children }) {
  const [hover, setHover] = useState(false);
  return (
    <span
      onClick={onClick}
      onMouseEnter={() => setHover(true)}
      onMouseLeave={() => setHover(false)}
      style={{
        display: "inline-flex",
        alignItems: "center",
        gap: 6,
        padding: "5px 12px",
        borderRadius: 999,
        fontSize: 13,
        border: `1px solid ${active ? "var(--color-forest)" : (hover ? "var(--border-strong)" : "var(--border)")}`,
        background: active ? "var(--color-forest)" : "var(--bg-1)",
        color: active ? "var(--color-paper)" : "var(--fg-1)",
        cursor: "pointer",
        transition: "all 120ms cubic-bezier(0.2,0,0,1)",
        userSelect: "none",
        fontWeight: active ? 500 : 400,
      }}
    >{children}</span>
  );
}

// ---------- StatusDot / StatusPill ----------
const STATUS_COLORS = {
  open: { c: "var(--color-forest-green)", label: "Open" },
  pending: { c: "var(--color-mandarin-orange)", label: "Awaiting review" },
  urgent: { c: "var(--color-jubilee-red)", label: "Closing today" },
  closed: { c: "var(--color-ink-muted)", label: "Closed" },
  accepted: { c: "var(--color-forest-green)", label: "Accepted" },
};

function StatusPill({ status, label, style }) {
  const s = STATUS_COLORS[status] || { c: "var(--fg-3)", label: status };
  return (
    <span style={{
      display: "inline-flex",
      alignItems: "center",
      gap: 6,
      fontFamily: "var(--font-mono)",
      fontSize: 11.5,
      fontWeight: 500,
      color: s.c,
      ...style,
    }}>
      <span style={{ width: 8, height: 8, borderRadius: "50%", background: s.c }}/>
      {label || s.label}
    </span>
  );
}

// ---------- Avatar ----------
// Avatar gradients drawn from the UoN supporting palette — paired with Nottingham
// Blue as the institutional anchor in each combination.
const AVATAR_GRADIENTS = [
  "linear-gradient(135deg, #DEB406, #B91C2E)",  // Rebel's Gold → Jubilee Red
  "linear-gradient(135deg, #10263B, #37B4B0)",  // Nottingham Blue → Trent Turquoise
  "linear-gradient(135deg, #792D85, #D7336C)",  // Civic Purple → Pioneering Pink
  "linear-gradient(135deg, #005F36, #DEB406)",  // Forest Green → Rebel's Gold
];

function Avatar({ initials, hue = 0, size = "md", style, onClick }) {
  const sizes = { sm: 28, md: 40, lg: 56, xl: 72 };
  const px = sizes[size] || size;
  return (
    <span
      onClick={onClick}
      style={{
        width: px,
        height: px,
        background: AVATAR_GRADIENTS[hue % AVATAR_GRADIENTS.length],
        color: "var(--color-paper)",
        display: "inline-flex",
        alignItems: "center",
        justifyContent: "center",
        fontWeight: 500,
        letterSpacing: "-0.01em",
        fontSize: px <= 28 ? 11 : px <= 40 ? 14 : px <= 56 ? 18 : 22,
        borderRadius: 4,
        flexShrink: 0,
        cursor: onClick ? "pointer" : "default",
        userSelect: "none",
        ...style,
      }}
    >{initials}</span>
  );
}

// ---------- MatchMeter ----------
function MatchMeter({ pct, color, width = 120 }) {
  let fill = color;
  if (!fill) {
    fill = pct >= 70 ? "var(--color-forest-green)" : pct >= 45 ? "var(--color-rebels-gold)" : "var(--color-ink-faint)";
  }
  return (
    <div style={{ display: "inline-flex", alignItems: "center", gap: 8 }}>
      <div style={{
        width,
        height: 6,
        background: "var(--bg-2)",
        borderRadius: 2,
        overflow: "hidden",
      }}>
        <div style={{
          width: `${pct}%`,
          height: "100%",
          background: fill,
          borderRadius: 2,
          transition: "width 240ms cubic-bezier(0.2,0,0,1)",
        }}/>
      </div>
      <span style={{
        fontFamily: "var(--font-mono)",
        fontSize: 12,
        fontVariantNumeric: "tabular-nums",
        color: "var(--fg-1)",
        minWidth: 32,
        textAlign: "right",
        fontWeight: 500,
      }}>{pct}%</span>
    </div>
  );
}

// ---------- MatchBadge (compact, for inline use) ----------
function MatchBadge({ pct }) {
  return (
    <span style={{
      display: "inline-flex",
      alignItems: "center",
      gap: 5,
      padding: "3px 9px",
      background: "var(--color-forest-green-20)",
      color: "var(--color-forest-green)",
      borderRadius: 4,
      fontFamily: "var(--font-mono)",
      fontSize: 11.5,
      fontWeight: 500,
    }}>{pct}% match</span>
  );
}

// ---------- TextInput ----------
function TextInput({ label, help, icon, ...props }) {
  const [focus, setFocus] = useState(false);
  return (
    <div style={{ display: "flex", flexDirection: "column", gap: 6 }}>
      {label && <label style={{ fontSize: 12, fontWeight: 500, color: "var(--fg-2)" }}>{label}</label>}
      <div style={{ position: "relative" }}>
        {icon && (
          <span style={{ position: "absolute", left: 12, top: "50%", transform: "translateY(-50%)", color: "var(--fg-3)", display: "flex" }}>
            <Icon name={icon} size={16}/>
          </span>
        )}
        <input
          {...props}
          onFocus={(e) => { setFocus(true); props.onFocus && props.onFocus(e); }}
          onBlur={(e) => { setFocus(false); props.onBlur && props.onBlur(e); }}
          style={{
            fontFamily: "var(--font-sans)",
            fontSize: 14,
            padding: icon ? "9px 12px 9px 36px" : "9px 12px",
            background: "var(--bg-1)",
            border: `1px solid ${focus ? "var(--color-forest)" : "var(--border-strong)"}`,
            outline: focus ? "1px solid var(--color-forest)" : "none",
            outlineOffset: -2,
            borderRadius: 4,
            color: "var(--fg-1)",
            width: "100%",
            boxSizing: "border-box",
            transition: "border-color 120ms cubic-bezier(0.2,0,0,1)",
          }}
        />
      </div>
      {help && <div style={{ fontSize: 11.5, color: "var(--fg-3)" }}>{help}</div>}
    </div>
  );
}

function Textarea({ label, help, rows = 4, ...props }) {
  const [focus, setFocus] = useState(false);
  return (
    <div style={{ display: "flex", flexDirection: "column", gap: 6 }}>
      {label && <label style={{ fontSize: 12, fontWeight: 500, color: "var(--fg-2)" }}>{label}</label>}
      <textarea
        rows={rows}
        {...props}
        onFocus={(e) => { setFocus(true); props.onFocus && props.onFocus(e); }}
        onBlur={(e) => { setFocus(false); props.onBlur && props.onBlur(e); }}
        style={{
          fontFamily: "var(--font-sans)",
          fontSize: 14,
          padding: "10px 12px",
          background: "var(--bg-1)",
          border: `1px solid ${focus ? "var(--color-forest)" : "var(--border-strong)"}`,
          outline: focus ? "1px solid var(--color-forest)" : "none",
          outlineOffset: -2,
          borderRadius: 4,
          color: "var(--fg-1)",
          width: "100%",
          boxSizing: "border-box",
          resize: "vertical",
          lineHeight: 1.5,
          transition: "border-color 120ms cubic-bezier(0.2,0,0,1)",
        }}
      />
      {help && <div style={{ fontSize: 11.5, color: "var(--fg-3)" }}>{help}</div>}
    </div>
  );
}

// ---------- Toast ----------
function Toast({ message, onDismiss }) {
  return (
    <div style={{
      position: "fixed",
      bottom: 24,
      left: "50%",
      transform: "translateX(-50%)",
      background: "var(--color-ink)",
      color: "var(--color-paper)",
      padding: "10px 16px",
      borderRadius: 6,
      fontSize: 14,
      display: "flex",
      alignItems: "center",
      gap: 12,
      zIndex: 100,
      boxShadow: "0 12px 32px -12px rgba(16,38,59,0.4)",
    }}>
      <Icon name="check" size={16}/>
      <span>{message}</span>
      <span style={{ cursor: "pointer", opacity: 0.7, marginLeft: 8 }} onClick={onDismiss}>
        <Icon name="x" size={14}/>
      </span>
    </div>
  );
}

Object.assign(window, {
  Icon, Eyebrow, Hairline, Button, Tag, Chip, StatusPill,
  Avatar, MatchMeter, MatchBadge, TextInput, Textarea, Toast,
  useViewport,
});

// ---------- useViewport hook ----------
function useViewport() {
  const [isMobile, setIsMobile] = React.useState(
    typeof window !== "undefined" && window.matchMedia("(max-width: 899px)").matches
  );
  React.useEffect(() => {
    const mq = window.matchMedia("(max-width: 899px)");
    const update = () => setIsMobile(mq.matches);
    mq.addEventListener("change", update);
    return () => mq.removeEventListener("change", update);
  }, []);
  return { isMobile };
}
window.useViewport = useViewport;
