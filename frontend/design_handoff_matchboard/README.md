# Handoff: Matchboard — Digital Research Service

## Overview

**Matchboard** is the Digital Research Service's skills-matching web app for the **University of Nottingham**. Researchers maintain a profile of expertise; project leads post project descriptions; the system surfaces high-signal matches across departments and disciplines. The product sits within UoN's IT function (DRS) and uses the official UoN brand palette.

This bundle contains everything needed to implement Matchboard — or any other DRS product — in a real codebase. The target is a **Next.js (App Router) + TypeScript** application, but the system is framework-agnostic; substitute Vue / SvelteKit / SwiftUI / etc. where appropriate.

## About the design files

The files in `ui-kit-reference/` are **design references created in HTML + React via in-browser Babel** — they are prototypes that show the intended look, layout, and interaction behaviour. They are **not production code** and should not be shipped as-is.

The implementation task is to **recreate these designs in your target codebase's environment** using its existing patterns and libraries — proper ES modules, real React component files, a real bundler, real routing, real data fetching. The CSS tokens in `design-system/tokens.css` and the assets in `design-system/assets/` *are* meant to ship as-is (or be ported into Tailwind, see below).

## Fidelity

**High-fidelity.** All colours, typography, spacing, border radii, and component states are final. Recreate the UI pixel-perfectly. Specifically:

- The colour system is locked to the official **University of Nottingham brand palette** (https://www.nottingham.ac.uk/brand/visual/colour.aspx) and should not be deviated from.
- The typography (Geist + Geist Mono) is final.
- Component visuals, spacing, and motion durations are all final.
- The exact React component split in `ui-kit-reference/` is a *suggestion* — refactor freely as long as the visual output matches.

## What's in this bundle

```
design_handoff_matchboard/
├── README.md                          ← this file (read first)
├── design-system/
│   ├── BRAND.md                       ← full brand guide: voice, motifs, motion, iconography
│   ├── SKILL.md                       ← top-rules cheat sheet (do/don't list)
│   ├── tokens.css                     ← all CSS variables — colours, type, spacing, radii, shadows
│   └── assets/
│       ├── uon-logo-blue.png          ← official UoN logo (screen resolution PNG)
│       ├── uon-logo-white.png         ← UoN logo for dark backgrounds
│       ├── drs-mark.svg               ← internal DRS placeholder mark (not for public use)
│       ├── drs-wordmark.svg
│       └── matchboard-wordmark.svg    ← product wordmark
└── ui-kit-reference/
    ├── PROTOTYPE_README.md            ← walkthrough of the prototype's structure
    ├── index.html                     ← prototype entry point (open in a browser to see it)
    ├── responsive.css                 ← layout breakpoints + helper classes
    ├── components.jsx                 ← primitives: Button, Tag, Avatar, Icon, useViewport, etc.
    ├── layout.jsx                     ← TopBar, LeftRail (with mobile drawer), PageHeader
    ├── cards.jsx                      ← ProjectCard, ResearcherCard, MatchRow
    ├── screens.jsx                    ← Discover / Project / Researcher / Post / Matches
    ├── app.jsx                        ← root component, manages routing + nav drawer
    └── data.jsx                       ← sample researchers, projects, disciplines
```

## Important — Brand rules (non-negotiable)

These come from the official UoN brand guide and must be honoured:

1. **Nottingham Blue (`#10263B`) must be visually dominant** on every asset.
2. **Black is not part of the palette.** All body text is Nottingham Blue or one of its tints (`#405162`, `#707D89`, `#9FA8B1`).
3. **Maximum two supporting colours per design.** Matchboard's chosen pair: **Forest Green `#005F36`** (strong matches / success) and **Rebel's Gold `#DEB406`** (skill tags / warm accent).
4. **White is not used as a digital background.** Use Portland Stone 40% tint `#FDFBF9` (preferred) or Nottingham Blue 5% tint `#F3F4F5`.
5. **No emoji in product UI.** Heroicons (outline, 1.5px stroke) only.
6. **Sentence case** for all UI labels. The only ALL-CAPS is a single eyebrow with `0.14em` tracking.
7. **Geist + Geist Mono** only. No other typefaces.
8. **Borders, not shadows**, define edges. Shadows reserved for popovers and modals.

`design-system/BRAND.md` has the long-form version with examples.

## Implementation guide — Next.js (App Router)

### Step 1: Drop in the tokens

Copy `design-system/tokens.css` into `app/globals.css` (or `styles/`) and import once at the top of `app/layout.tsx`:

```tsx
// app/layout.tsx
import "./globals.css";
```

The first line of `tokens.css` imports Geist from Google Fonts — **remove it** and use `next/font` instead (see step 2). Otherwise the file ships as-is.

### Step 2: Fonts via `next/font`

```tsx
// app/layout.tsx
import { Geist, Geist_Mono } from "next/font/google";

const geist = Geist({ subsets: ["latin"], variable: "--font-geist" });
const geistMono = Geist_Mono({ subsets: ["latin"], variable: "--font-geist-mono" });

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="en" className={`${geist.variable} ${geistMono.variable}`}>
      <body>{children}</body>
    </html>
  );
}
```

Then in `tokens.css`, update the font variables to point at the next/font CSS variables:

```css
--font-sans:    var(--font-geist), ui-sans-serif, system-ui, sans-serif;
--font-mono:    var(--font-geist-mono), ui-monospace, monospace;
--font-display: var(--font-geist), ui-sans-serif, system-ui, sans-serif;
```

### Step 3: Assets

Move everything in `design-system/assets/` into `public/`. References then become:

```tsx
import Image from "next/image";

<Image src="/uon-logo-blue.png" alt="University of Nottingham" width={170} height={64}/>
```

The prototype uses screen-resolution PNGs of the UoN logo — for production, **request the master SVG/EPS files from the UoN brand team** (brand@nottingham.ac.uk).

### Step 4: Icons via `@heroicons/react`

```bash
npm install @heroicons/react
```

The prototype inlines a handful of Heroicons in `components.jsx`. Replace the entire `ICONS` map and `<Icon name="..."/>` wrapper with direct imports:

```tsx
import { MagnifyingGlassIcon, BellIcon, SparklesIcon } from "@heroicons/react/24/outline";
```

All icons are outline / 24×24 / 1.5px stroke / `currentColor` — identical to the prototype.

### Step 5: Port components — mechanical conversion

Each prototype file becomes a real TypeScript module. The conversion is mechanical:

| Prototype | Next.js |
|---|---|
| `<script type="text/babel" src="components.jsx">` | One file per component under `components/` |
| `const { useState } = React;` | `import { useState } from "react";` |
| `Object.assign(window, {...})` | Delete entirely. Use named exports: `export function Button(...)` |
| `window.useViewport = useViewport;` | `export function useViewport(...)` |
| Inline styles + `var(--color-...)` | Keep working as-is (CSS variables are global) — or convert to Tailwind classes |
| `responsive.css` `.mb-two-col` etc. | Keep, or replace with Tailwind: `grid grid-cols-1 md:grid-cols-[1fr_320px] gap-10` |
| Babel-in-browser | Gone — Next.js compiles for you |

See `example-component-Button.tsx` in this folder for the exact pattern.

### Step 6: Replace routing

The prototype runs a manual `useState("discover")` router in `app.jsx`. In Next.js, this becomes:

```
app/
  page.tsx                     ← DiscoverScreen
  projects/[id]/page.tsx       ← ProjectDetailScreen
  researchers/[id]/page.tsx    ← ResearcherScreen
  post/page.tsx                ← PostProjectScreen
  matches/page.tsx             ← MatchesScreen
  layout.tsx                   ← TopBar + LeftRail + navOpen state
```

Replace internal navigation:
- `onClick={() => openProject(p.id)}` → `<Link href={`/projects/${p.id}`}>`
- `navigate("discover")` → `router.push("/")` from `next/navigation`

### Step 7: Data

`data.jsx` is sample data. Replace with a real source — API route, Server Component fetch, database, whatever the rest of the app uses. Typing-wise, mirror the shapes already used in `data.jsx`.

### Optional: Tailwind theme

If the existing codebase uses Tailwind, expose the UoN tokens in `tailwind.config.js`:

```js
// tailwind.config.example.js — included in this folder
module.exports = {
  theme: {
    extend: {
      colors: {
        "nottingham-blue":     "#10263B",
        "nottingham-blue-80":  "#405162",
        "nottingham-blue-60":  "#707D89",
        "nottingham-blue-40":  "#9FA8B1",
        "nottingham-blue-20":  "#CFD4D8",
        "nottingham-blue-5":   "#F3F4F5",

        "forest-green":        "#005F36",
        "rebels-gold":         "#DEB406",
        "jubilee-red":         "#B91C2E",
        "mandarin-orange":     "#F98109",
        "pioneering-pink":     "#D7336C",
        "civic-purple":        "#792D85",
        "bramley-apple":       "#93D500",
        "trent-turquoise":     "#37B4B0",
        "malaysia-blue":       "#009BC1",

        "portland-stone":      "#FAF6EF",
        "paper":               "#FDFBF9",
      },
      fontFamily: {
        sans:    ["var(--font-geist)",     "ui-sans-serif", "system-ui", "sans-serif"],
        mono:    ["var(--font-geist-mono)", "ui-monospace",  "monospace"],
        display: ["var(--font-geist)",     "ui-sans-serif", "system-ui", "sans-serif"],
      },
      borderRadius: {
        xs:   "2px",
        sm:   "4px",
        md:   "6px",
        lg:   "8px",
      },
    },
  },
};
```

## Screens / views

Each screen below corresponds to a function in `ui-kit-reference/screens.jsx`. Open `index.html` in a browser to see them live.

### 1. Discover — `app/page.tsx`

**Purpose:** Personalised feed of project matches.

**Layout:** Full-width screen with `mb-screen` padding (`32px 40px` desktop, `20px 16px` mobile). Page header (eyebrow + title + description), filter row (filter icon + chips + sort dropdown), then a `repeat(auto-fill, minmax(420px, 1fr))` grid of `ProjectCard`s (collapses to 1 column on mobile).

**Components used:** `PageHeader`, `Chip` (filter), `ProjectCard`, `Icon` (filter, chevronDown).

**Empty state:** When no projects match, show `EmptyState` with a CTA to edit the user's profile.

### 2. Project detail — `app/projects/[id]/page.tsx`

**Purpose:** View a project, request to join, see ranked candidate researchers.

**Layout:** `mb-screen` padding. Back-link button. `PageHeader` with eyebrow (`{discipline} · {code}`), title, meta row (status, posted, duration, funded, match%), and a "Request to join" CTA. Below: a **two-column** grid (`mb-two-col`: `1fr 320px`, stacks on mobile):

- **Left:** Project long description → Required skills (gold tags) → Nice-to-have (neutral tags) → Suggested candidates list using `MatchRow`.
- **Right (sticky):** Project lead card + at-a-glance metadata table (code, posted, deadline, duration, funded, candidates).

**Interactive:** Clicking "Request to join" updates local state and shows a `Toast` ("Request sent to project lead").

### 3. Researcher profile — `app/researchers/[id]/page.tsx`

**Purpose:** A researcher's expertise, publications, open collaborations.

**Layout:** Back-link. Header row (`mb-profile-row`): xl Avatar + name/title/discipline + actions (Send message / Save / ellipsis). On the right (`mb-profile-stats`), three stat blocks (publications / h-index / open collabs). Below the hairline divider: a two-column grid (`mb-two-col-narrow`: `1fr 280px`) with About + Recent publications on the left and Skills (gold tags) on the right.

### 4. Post project — `app/post/page.tsx`

**Purpose:** Composer for creating a new project.

**Layout:** `mb-screen` with `maxWidth: 760`. Back-link. `PageHeader`. Form (`TextInput`, `Textarea`) with fields: project title, primary discipline, expected duration, description, required skills (comma-separated). Submit button posts and toasts.

### 5. Matches — `app/matches/page.tsx`

**Purpose:** Incoming and outgoing match requests.

**Layout:** `PageHeader`. Tab strip ("Incoming" + count badge, "Outgoing" + count badge) with bottom-border active indicator. Below: list of cards showing project metadata + status pill or accept/decline buttons.

## Components reference

### Primitives (`components.jsx`)

- **`Icon`** — wraps inline Heroicon SVGs. **Replace with `@heroicons/react`.**
- **`Eyebrow`** — uppercase small-caps label, 11px, `letter-spacing: 0.14em`, default colour `var(--fg-3)`.
- **`Hairline`** — 1px divider, defaults to horizontal full-width.
- **`Button`** — variants: `primary` (Nottingham Blue / Paper text), `secondary` (Paper / Blue text / Blue-tint border), `ghost` (transparent / Blue text), `destructive` (Paper / Jubilee Red text / Red-tint border). Sizes: `sm` / `md` / `lg`. Optional `icon` (left) and `iconRight`. Press animation: `scale(0.98)` for 80ms.
- **`Tag`** — variants: `skill` (Rebel's Gold tint background, Nottingham Blue text), `discipline` (Blue tint background, Blue text), `neutral` (paper-deep background, soft text). 2px border radius, mono 12px text.
- **`Chip`** — filter pill, 999px radius, active state: Nottingham Blue background with Paper text.
- **`StatusPill`** — coloured dot + label. States: `open` (Forest Green), `pending` (Mandarin Orange), `urgent` (Jubilee Red), `closed` (muted), `accepted` (Forest Green).
- **`Avatar`** — sizes `sm` (28), `md` (40), `lg` (56), `xl` (72). 4 gradient variants drawn from UoN supporting palette. 4px radius. Initials shown in Paper colour.
- **`MatchMeter`** — horizontal bar + percentage. Fill colour: ≥70% Forest Green, ≥45% Rebel's Gold, else Ink-faint.
- **`MatchBadge`** — compact match% pill in Forest Green on Forest Green tint.
- **`TextInput`** / **`Textarea`** — label + input. Focus ring is Nottingham Blue. Optional left icon, optional helper text below.
- **`Toast`** — bottom-centred dismissible notification. Nottingham Blue background, Paper text.
- **`useViewport()`** — returns `{ isMobile }`, true when `matchMedia('(max-width: 899px)')` matches.

### Layout (`layout.jsx`)

- **`TopBar`** — sticky header, 60px tall.
  - **Desktop:** UoN logo (34px) → 1px divider → Matchboard wordmark → "Digital Research Service" label → search → spacer → Post project button → bell → avatar.
  - **Mobile (≤899px):** hamburger → UoN logo (28px) → compact Matchboard wordmark → spacer → icon-only Post button → avatar.
- **`LeftRail`** — primary nav.
  - **Desktop:** 220px sticky sidebar with nav items + "Disciplines" subsection.
  - **Mobile:** slide-out drawer (280px, transform-driven, 220ms ease-out) with scrim overlay. Triggered by the TopBar hamburger, dismissed by scrim click, close button, or route change.
- **`PageHeader`** — eyebrow, title, optional description, optional meta row, optional action slot. Title size dampens from 36px → 28px → 24px at the `md` and `sm` breakpoints (handled by `.mb-page-title` in `responsive.css`).
- **`EmptyState`** — centred container with circular icon background, title, description, optional action.

### Composite cards (`cards.jsx`)

- **`ProjectCard`** — used on Discover and Matches. 6px border-radius card, 22px padding. Top row: eyebrow ({discipline · status}) + match badge. Title, description, skill tags, then a hairline footer with lead avatar + lead name + posted-ago + (funded · duration). Border lightens to `--border-strong` on hover.
- **`ResearcherCard`** — full or `dense`. lg avatar + name/title + match badge + bio + skill tags + meta row (pubs, h-index, open collabs).
- **`MatchRow`** — compact row used in candidate ranking lists: avatar, name + match reasons, `MatchMeter`, chevron-right.

## Design tokens (extracted)

### Colours

See `design-system/tokens.css` for the canonical list with semantic aliases. The full UoN palette is documented at https://www.nottingham.ac.uk/brand/visual/colour.aspx.

| Token | Hex | Use |
|---|---|---|
| `--color-nottingham-blue` | `#10263B` | Primary brand, body text, key marks |
| `--color-nottingham-blue-80` | `#405162` | Secondary text |
| `--color-nottingham-blue-60` | `#707D89` | Tertiary / metadata text |
| `--color-nottingham-blue-40` | `#9FA8B1` | Disabled / placeholder |
| `--color-nottingham-blue-20` | `#CFD4D8` | Borders, light surfaces |
| `--color-nottingham-blue-5` | `#F3F4F5` | Subtle bg alternative |
| `--color-forest-green` | `#005F36` | Success, strong matches, "Funded" |
| `--color-forest-green-20` | `#CCDFD7` | Success tint |
| `--color-rebels-gold` | `#DEB406` | Warm accent, skill-tag indicator |
| `--color-rebels-gold-20` | `#F8F0CD` | Skill tag background |
| `--color-jubilee-red` | `#B91C2E` | Destructive, error |
| `--color-jubilee-red-20` | `#F1D2D5` | Error tint |
| `--color-mandarin-orange` | `#F98109` | Warning |
| `--color-portland-stone` | `#FAF6EF` | Inset surface |
| `--color-portland-stone-40` | `#FDFBF9` | **Universal body background** |

The supporting palette also includes Pioneering Pink (`#D7336C`), Civic Purple (`#792D85`), Bramley Apple (`#93D500`), Trent Turquoise (`#37B4B0`), Malaysia Sky Blue (`#009BC1`) — available for other DRS products but not used in Matchboard (max-2-supporting rule).

### Typography

- **Geist** for all UI, body, display.
- **Geist Mono** for tabular data, identifiers, codes.

Scale: 12 / 13 / 16 / 17 / 20 / 24 / 32 / 44 / 60 / 84 px. Weights used: 400 / 500 / 600. Letter-spacing: `-0.035em` (display) → `-0.015em` (body) → `+0.14em` (eyebrow uppercase).

### Spacing

4px base. Tokens: 4 / 8 / 12 / 16 / 24 / 32 / 48 / 64 / 96 px.

### Radii

- `--radius-xs: 2px` (tags)
- `--radius-sm: 4px` (buttons, inputs)
- `--radius-md: 6px` (cards)
- `--radius-lg: 8px` (modals)
- `--radius-pill: 999px` (filter chips, dots)

### Shadows

```css
--shadow-popover: 0 6px 24px -8px rgba(16, 38, 59, 0.18);
--shadow-modal:   0 24px 64px -24px rgba(16, 38, 59, 0.32);
```

Everywhere else: 1px border in `var(--color-hairline)` (rgba(16, 38, 59, 0.14)). No drop shadows on cards.

### Motion

- **Duration:** 120ms (default), 180ms (drawers), 80ms (button press).
- **Easing:** `cubic-bezier(0.2, 0, 0, 1)` for all UI transitions.
- **No bounces, no slide-ins on content.**

### Breakpoints

- `--bp-sm: 640px` — page-header title dampens further.
- `--bp-md: 900px` — main reflow point. Two-column screens stack, left rail becomes drawer, top bar collapses, screen padding tightens.
- `--bp-lg: 1200px` — max content width on app surfaces.

## Interactions & state

- **Top-bar logo** → home (Discover).
- **Hamburger (mobile)** → opens LeftRail drawer (state: `navOpen`).
- **Project card** → project detail.
- **Researcher name** → researcher profile.
- **"Post project"** → composer.
- **Request to join** → toast + local state flips to "Request sent".
- **Drawer auto-closes** on route change (via `useEffect` on the route prop).
- **Filter chips** — horizontal-scroll on mobile, wrap on desktop.

## Accessibility notes

- **Contrast:** The 60% Nottingham Blue tint (`#707D89`) on Portland Stone 40% (`#FDFBF9`) is ~3.7:1 — meets WCAG AA for large text but fails AA for small body text. The prototype uses it only for metadata (12px). For strict AA compliance across all text, swap metadata to the 80% tint (`#405162`, ~7.4:1, AAA).
- **Focus rings:** All inputs have a visible Nottingham Blue focus outline (`outline: 1px solid var(--color-primary)` + matching border). Buttons inherit browser focus — add a custom ring if you remove it.
- **Aria labels:** All icon-only buttons in the prototype carry `aria-label`. Preserve these in the port.
- **Reduced motion:** Not yet implemented — wrap transitions in `@media (prefers-reduced-motion: reduce)` if your audience needs it.

## Things to do *not* in this bundle

These were out of scope for the prototype and need to be designed/built downstream:

- Real authentication / settings screens.
- Search results page (the top-bar search input is decorative in the prototype).
- Researcher profile editor (you can only *view* profiles in the prototype).
- Admin / staff dashboard for DRS internal use.
- Onboarding flow.
- Notifications / activity feed (the bell icon is decorative).
- Real photography / headshots (avatars are gradient + initials).
- Print stylesheet.
- i18n (UoN is multi-campus — UK / China / Malaysia).

## Questions to resolve with UoN brand team

- **Get the master logo files** (SVG / EPS / vector). The PNGs in this bundle are screen-resolution only.
- **Confirm the digital hover state** for Nottingham Blue. The prototype uses `#081827` (darker shade); not in the official palette.
- **Confirm minimum logo sizes** and clear-space rules.
- **Confirm body-text contrast policy** — does the brand team accept AA, or do they require AAA?
- **Confirm font licensing** — Geist is open-source (Vercel / OFL) so this should be fine, but UoN may have a preferred typeface for production digital products (the brand site has a Fonts page worth checking).

## Files in this bundle — quick reference

| Path | What it is | Ships? |
|---|---|---|
| `design-system/tokens.css` | All design tokens as CSS vars | **Yes, drop into `globals.css`** |
| `design-system/BRAND.md` | Long-form brand guide | Read it; don't ship |
| `design-system/SKILL.md` | Top-rules cheat sheet | Read it; don't ship |
| `design-system/assets/uon-logo-*.png` | UoN logos | **Yes, into `public/`** (replace with masters in prod) |
| `design-system/assets/matchboard-wordmark.svg` | Product wordmark | **Yes, into `public/`** |
| `ui-kit-reference/*.jsx` | Prototype source | **No** — use as a recipe, write real `.tsx` |
| `ui-kit-reference/responsive.css` | Layout breakpoint helpers | Optional — port to Tailwind, or include directly |
| `ui-kit-reference/index.html` | Prototype entry | **No** — open in a browser as a visual reference |
| `tailwind.config.example.js` | Tailwind theme for the UoN palette | Use if Tailwind is your styling layer |
| `example-component-Button.tsx` | Sample TSX port of the prototype's `Button` | Pattern to follow for the rest |

---

If anything in this doc is unclear, or if you want me to also generate full `.tsx` ports of the rest of the components, ask.
