import type { Config } from "tailwindcss";

/**
 * Tailwind theme wired to the University of Nottingham brand palette.
 * CSS variables are the source of truth (app/globals.css :root block);
 * these names are convenient utility-class handles over the same values.
 *
 * Colour usage rules from the UoN brand guide:
 *   - Nottingham Blue must be visually dominant on every asset.
 *   - Maximum two supporting colours per design.
 *   - Black is not part of the palette — use Nottingham Blue or its tints.
 *   - White is not used as a digital background — use `paper` (#FDFBF9) or
 *     `nottingham-blue-5` (#F3F4F5).
 */
const config: Config = {
  content: ["./app/**/*.{ts,tsx}", "./components/**/*.{ts,tsx}"],
  corePlugins: {
    // globals.css provides all base/reset styles; disable Tailwind Preflight
    // to avoid conflicts with the existing element styles.
    preflight: false,
  },
  theme: {
    extend: {
      colors: {
        // --- UoN Primary ---
        "nottingham-blue": "#10263B",
        "nottingham-blue-80": "#405162",
        "nottingham-blue-60": "#707D89",
        "nottingham-blue-40": "#9FA8B1",
        "nottingham-blue-20": "#CFD4D8",
        "nottingham-blue-5": "#F3F4F5",

        // --- UoN Supporting (max 2 per asset) ---
        "forest-green": "#005F36",
        "forest-green-20": "#CCDFD7",
        "rebels-gold": "#DEB406",
        "rebels-gold-20": "#F8F0CD",
        "jubilee-red": "#B91C2E",
        "jubilee-red-20": "#F1D2D5",
        "mandarin-orange": "#F98109",
        "mandarin-orange-20": "#FEE6CE",
        "pioneering-pink": "#D7336C",
        "pioneering-pink-20": "#F7D6E2",
        "civic-purple": "#792D85",
        "civic-purple-20": "#E4D5E7",
        "bramley-apple": "#93D500",
        "bramley-apple-20": "#E9F7CC",
        "trent-turquoise": "#37B4B0",
        "trent-turquoise-20": "#D7F0EF",
        "malaysia-blue": "#009BC1",
        "malaysia-blue-20": "#CCEBF3",

        // --- UoN Neutral ---
        "portland-stone": "#FAF6EF",
        paper: "#FDFBF9",

        // --- Semantic roles ---
        ink: "#10263B",
        "ink-soft": "#405162",
        "ink-muted": "#707D89",
        "ink-faint": "#9FA8B1",
      },

      fontFamily: {
        // CSS variables are injected by next/font in app/layout.tsx.
        sans: ["var(--font-geist)", "ui-sans-serif", "system-ui", "sans-serif"],
        mono: ["var(--font-geist-mono)", "ui-monospace", "monospace"],
        display: [
          "var(--font-geist)",
          "ui-sans-serif",
          "system-ui",
          "sans-serif",
        ],
      },

      fontSize: {
        xs: ["12px", { lineHeight: "1.5" }],
        sm: ["13px", { lineHeight: "1.5" }],
        base: ["16px", { lineHeight: "1.55" }],
        md: ["17px", { lineHeight: "1.55" }],
        lg: ["20px", { lineHeight: "1.4" }],
        xl: ["24px", { lineHeight: "1.3", letterSpacing: "-0.02em" }],
        "2xl": ["32px", { lineHeight: "1.2", letterSpacing: "-0.025em" }],
        "3xl": ["44px", { lineHeight: "1.1", letterSpacing: "-0.03em" }],
        "4xl": ["60px", { lineHeight: "1.05", letterSpacing: "-0.03em" }],
        "5xl": ["84px", { lineHeight: "0.98", letterSpacing: "-0.035em" }],
      },

      borderRadius: {
        xs: "2px",
        sm: "4px",
        md: "6px",
        lg: "8px",
        pill: "9999px",
      },

      boxShadow: {
        popover: "0 6px 24px -8px rgba(16, 38, 59, 0.18)",
        modal: "0 24px 64px -24px rgba(16, 38, 59, 0.32)",
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
        md: "900px",
        lg: "1200px",
      },

      maxWidth: {
        reading: "65ch",
        app: "1200px",
      },
    },
  },
  plugins: [],
};

export default config;
