// tailwind.config.example.js
// Drop this into your Next.js project root (or merge into an existing config).
// Pairs with design-system/tokens.css — the CSS variables are the source of truth,
// these Tailwind names are just convenient handles.

/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./app/**/*.{ts,tsx}",
    "./components/**/*.{ts,tsx}",
    "./pages/**/*.{ts,tsx}",  // remove if app router only
  ],
  theme: {
    extend: {
      colors: {
        // --- UoN Primary ---
        "nottingham-blue":     "#10263B",
        "nottingham-blue-80":  "#405162",
        "nottingham-blue-60":  "#707D89",
        "nottingham-blue-40":  "#9FA8B1",
        "nottingham-blue-20":  "#CFD4D8",
        "nottingham-blue-5":   "#F3F4F5",

        // --- UoN Supporting (max 2 per asset!) ---
        "forest-green":        "#005F36",
        "forest-green-20":     "#CCDFD7",
        "rebels-gold":         "#DEB406",
        "rebels-gold-20":      "#F8F0CD",
        "jubilee-red":         "#B91C2E",
        "jubilee-red-20":      "#F1D2D5",
        "mandarin-orange":     "#F98109",
        "mandarin-orange-20":  "#FEE6CE",
        "pioneering-pink":     "#D7336C",
        "civic-purple":        "#792D85",
        "bramley-apple":       "#93D500",
        "trent-turquoise":     "#37B4B0",
        "malaysia-blue":       "#009BC1",

        // --- UoN Neutral ---
        "portland-stone":      "#FAF6EF",
        "paper":               "#FDFBF9",  // = Portland Stone 40%, the body bg

        // --- Semantic roles ---
        ink:       "#10263B",
        "ink-soft":  "#405162",
        "ink-muted": "#707D89",
        "ink-faint": "#9FA8B1",
      },
      fontFamily: {
        // Wire these CSS variables in app/layout.tsx via next/font:
        //   const geist = Geist({ subsets: ["latin"], variable: "--font-geist" });
        //   const geistMono = Geist_Mono({ subsets: ["latin"], variable: "--font-geist-mono" });
        sans:    ["var(--font-geist)",      "ui-sans-serif", "system-ui", "sans-serif"],
        mono:    ["var(--font-geist-mono)", "ui-monospace",  "monospace"],
        display: ["var(--font-geist)",      "ui-sans-serif", "system-ui", "sans-serif"],
      },
      fontSize: {
        // Custom scale matching tokens.css
        xs:   ["12px", { lineHeight: "1.5" }],
        sm:   ["13px", { lineHeight: "1.5" }],
        base: ["16px", { lineHeight: "1.55" }],
        md:   ["17px", { lineHeight: "1.55" }],
        lg:   ["20px", { lineHeight: "1.4" }],
        xl:   ["24px", { lineHeight: "1.3", letterSpacing: "-0.02em" }],
        "2xl":["32px", { lineHeight: "1.2", letterSpacing: "-0.025em" }],
        "3xl":["44px", { lineHeight: "1.1", letterSpacing: "-0.03em" }],
        "4xl":["60px", { lineHeight: "1.05", letterSpacing: "-0.03em" }],
        "5xl":["84px", { lineHeight: "0.98", letterSpacing: "-0.035em" }],
      },
      borderRadius: {
        xs:   "2px",
        sm:   "4px",
        md:   "6px",
        lg:   "8px",
        pill: "9999px",
      },
      boxShadow: {
        popover: "0 6px 24px -8px rgba(16, 38, 59, 0.18)",
        modal:   "0 24px 64px -24px rgba(16, 38, 59, 0.32)",
      },
      transitionTimingFunction: {
        "ease-out-uon": "cubic-bezier(0.2, 0, 0, 1)",
      },
      transitionDuration: {
        fast: "120ms",
        base: "180ms",
      },
      screens: {
        sm: "640px",
        md: "900px",   // ← primary reflow point (not Tailwind's default 768)
        lg: "1200px",
      },
      maxWidth: {
        reading: "65ch",
        app:     "1200px",
      },
    },
  },
  plugins: [],
};
