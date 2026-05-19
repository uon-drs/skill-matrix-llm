# Digital Research Service — Design System

**Digital Research Service (DRS)** is an innovative university IT department building tools that help researchers form cross-disciplinary teams. The product surfaced in this design system is a **skills-matching web app**: researchers maintain a profile of their expertise, project leads post project descriptions, and the system surfaces high-signal matches across departments and disciplines.

The brand wants to feel like reading *Nature* or *Wellcome Trust* publications — editorial, restrained, data-forward, intellectually serious, but warm enough to invite people from disparate fields to actually collaborate.

## Sources

This system was bootstrapped from scratch at the user's request — there was no existing codebase, Figma, or full brand reference attached. After v1, the user confirmed they work at the **University of Nottingham**, the institutional parent, and supplied the official Nottingham screen logos (`assets/uon-logo-blue.png` and `assets/uon-logo-white.png`). UoN navy `#10263b` was sampled from the supplied logo and added as a supplementary token. Substitute or override anything else here that conflicts with the official Nottingham brand guidelines.

## Brand hierarchy

- **University of Nottingham** — institutional parent. Real, trademarked brand. Lock-up in `assets/uon-logo-*.png`. Always present in the top bar / footer of any DRS surface.
- **Digital Research Service (DRS)** — the department within UoN's IT function that owns Matchboard. Appears as a small mono text label, not a competing logo.
- **Matchboard** — the product. Has its own wordmark in `assets/matchboard-wordmark.svg`, paired with UoN in a co-brand lock-up (UoN | Matchboard).

The DRS mark in `assets/drs-mark.svg` is an internal placeholder — not for public-facing use unless the department adopts it formally.

---

## Index

| File / Folder | Purpose |
|---|---|
| `README.md` | This file. Brand context, content rules, visual foundations, iconography. |
| `SKILL.md` | Agent Skill entry point — read this when invoking the skill. |
| `colors_and_type.css` | All design tokens — colors, typography, spacing, radii, shadows. |
| `fonts/` | Webfont references (Geist via Google Fonts). |
| `assets/` | Logos, wordmarks, sample researcher headshots, illustrations. |
| `preview/` | Card files surfaced in the Design System tab. |
| `ui_kits/matchboard/` | UI kit for the skills-matching web app ("Matchboard"). |
| `ui_kits/matchboard/index.html` | Interactive multi-screen prototype. |
| `ui_kits/matchboard/*.jsx` | Reusable component files. |

---

## Content Fundamentals

**Voice.** Like a well-edited research publication: factual, specific, never breathless. Sentences carry information; adjectives earn their place. The reader is a busy scientist — respect their time.

**Person.** Address the user as *you*, but speak about the service in the third person ("Matchboard surfaces researchers whose recent work overlaps with your project"). Avoid "we" except in legal / staff-authored copy.

**Casing.** Sentence case everywhere — buttons, menu items, headings, navigation. The only Title Case Things are proper nouns (research areas, lab names, the product name "Matchboard"). Never ALL CAPS except in a single editorial eyebrow (e.g. `RECENT MATCHES`) at small sizes with generous letter-spacing.

**Numbers and data.** Numerals always, even at the start of sentences if necessary ("3 researchers matched"). Use the thin-space thousands separator visually but plain digits in inputs. Percentages have no space (`87%`). Always show units.

**Tone — specific examples.**
- ✅ "Match strength: 87%. Overlap on bioinformatics, single-cell sequencing, ethics review."
- ❌ "Wow, 87% — what a great match! 🎉"
- ✅ "No matches yet. Add a method or technique to your project to broaden the search."
- ❌ "Oops, nothing here! Try adding more tags."
- ✅ "Posted 4 days ago by Dr. Adaeze Okonkwo (Materials Science)."
- ❌ "Posted by Adaeze, 4 days ago!!"

**Emoji.** Never in product UI. Acceptable in informal staff-authored content (release notes, internal blog posts) but should still feel sparing.

**Empty states.** Diagnostic, not cute. State what's missing and what to do, in one sentence.

**Capitalization of disciplines.** Always Title Case for named fields (Computational Biology, Materials Science, Medieval Studies). Lowercase for general descriptors (interdisciplinary, quantitative methods).

---

## Visual Foundations

### Color

The colour system follows the **University of Nottingham brand palette** (see [nottingham.ac.uk/brand/visual/colour.aspx](https://www.nottingham.ac.uk/brand/visual/colour.aspx)). The full set of UoN tokens lives in `colors_and_type.css` under "UoN PRIMARY", "UoN SUPPORTING", and "UoN NEUTRAL" blocks.

**Rules from the UoN brand site — enforced here:**

- **Nottingham Blue must be visually dominant** on every asset. Body copy, headings, and the primary brand mark are all Nottingham Blue.
- **Black is not used.** Any historical black in the system has been replaced with Nottingham Blue.
- **Maximum two supporting colours per design**, and together they should account for less than half the colour make-up. Supporting colours are accents — illustrations, icons, calls-to-action — not primary surfaces.
- **Body copy must be Nottingham Blue or white.** No supporting colour appears as body text.
- **White is avoided as a digital background.** Use Portland Stone 40% tint (`#FDFBF9`) or Nottingham Blue 5% tint (`#F3F4F5`) instead.

**Primary** — Nottingham Blue `#10263B`. Five official tints: 80% `#405162`, 60% `#707D89`, 40% `#9FA8B1`, 20% `#CFD4D8`, 5% `#F3F4F5`.

**Supporting palette (9, max 2 per design):**

| Name | Hex |
|---|---|
| Jubilee Red | `#B91C2E` |
| Mandarin Orange | `#F98109` |
| Rebel's Gold | `#DEB406` |
| Pioneering Pink | `#D7336C` |
| Civic Purple | `#792D85` |
| Forest Green | `#005F36` |
| Bramley Apple | `#93D500` |
| Trent Turquoise | `#37B4B0` |
| Malaysia Sky Blue | `#009BC1` |

**Matchboard's chosen pair:** Forest Green (strong matches, success, "Funded") + Rebel's Gold (skill tags, partial matches, warm accent). Pick a different pair for other DRS products as long as you stick to two.

**Neutral** — Portland Stone `#FAF6EF` and its 40% tint `#FDFBF9` (preferred body background). White `#FFFFFF` for text on dark surfaces only.

**Semantic mapping** (system uses these):

- Success → Forest Green
- Warning → Mandarin Orange
- Error / destructive → Jubilee Red
- Info → Nottingham Blue

### Typography

**Geist** for everything (UI, body, display) and **Geist Mono** for tabular data, codes, and identifiers. Single-family discipline keeps the system feeling editorial — variation comes from weight (400, 500, 600), size, and tracking, not from family-mixing.

- Display headings: tight tracking (`-0.02em` to `-0.04em`), generous size jumps.
- Body: `1rem` (16px) at `1.55` line-height, optical kerning enabled.
- Small caps eyebrows: `0.75rem` Geist Medium, `0.14em` letter-spacing, uppercase.
- Numerals: tabular figures by default in data contexts (`font-variant-numeric: tabular-nums`).

### Spacing & Layout

A 4px base scale: `4, 8, 12, 16, 24, 32, 48, 64, 96`. Page gutters are generous — content columns rarely exceed 720px for reading or 1200px for app surfaces. Generous white space is a feature, not a bug.

Most layouts are left-aligned, asymmetric, with a clear primary column and a narrower secondary rail. Centered layouts are reserved for empty states and modals.

### Backgrounds

- **Default:** flat Paper (`#fdfcf8`). No textures, no gradients.
- **Section breaks:** a single 1px Ink-at-12% hairline — the editorial rule.
- **Cards:** Paper, with a 1px hairline border in Ink-at-10%. No drop shadows in the default state.
- **Imagery:** when used, treated as editorial photography — desaturated slightly, warm-leaning. Headshots are square or 4:5, never circular except in dense lists.

### Animation

Restrained. Interaction feedback is fast (120ms) and uses a custom ease (`cubic-bezier(0.2, 0, 0, 1)`). Page transitions are static — no slide-ins. Loading uses a single 1px progress bar at the top of the viewport (Forest), not spinners. Avoid bounces entirely.

### Hover States

- **Buttons:** background darkens by ~6% (Forest → darker Forest). Never lighten.
- **Links:** underline appears (was absent). Color unchanged.
- **Cards:** border shifts from Ink-at-10% to Ink-at-25%. No lift, no shadow.
- **Icon buttons:** background fills with Ink-at-6%.

### Press States

A subtle scale-down (`scale(0.98)`) on buttons only. No color change beyond hover. Released animation is 80ms.

### Borders

Always 1px. Always Ink-at-N% — never Forest as a border on body content (Forest borders are reserved for active/selected states). Border-radius is restrained: `4px` for inputs/buttons, `6px` for cards, `2px` for tags. No fully-rounded pills except for filter chips.

### Shadows

Almost none. The system uses **borders, not shadows** to define edges. Two exceptions:
- **Floating menus / popovers:** a single soft shadow — `0 6px 24px -8px rgba(26, 26, 26, 0.18)`.
- **Modals:** `0 24px 64px -24px rgba(26, 26, 26, 0.28)`.

No inner shadows anywhere.

### Capsules vs Protection Gradients

Capsules are used for tags, status chips, and filter pills. Protection gradients (white-to-transparent fades over imagery) are not used in this brand — imagery is always supported by adjacent text, not overlaid.

### Layout Rules

- App chrome is fixed (top bar + left rail). Content scrolls.
- Maximum content width on app surfaces: 1200px.
- Reading column: 65ch (~640px).
- Sticky elements use the Paper background with a 1px hairline; never blurred backdrops in product UI.

### Transparency & Blur

Used sparingly. Modal scrim is `rgba(26, 26, 26, 0.4)`, no blur. Toast notifications and the top bar are solid Paper.

### Imagery

Editorial documentary style: real researchers in real labs/offices, mid-action, warm color cast, slight film grain in hero contexts. Avoid stock photography clichés (gleaming microscopes, glowing molecules, hands-on-keyboards). When real photography isn't available, use **placeholders with a clear caption** — never AI-generated stand-ins.

### Cards

Paper background, 1px Ink-at-10% border, 6px radius, no shadow. Internal padding `24px`. Title row at top in Geist Medium. Metadata in Geist Mono small caps at the bottom.

---

## Iconography

**Heroicons** — line variant (1.5px stroke, 24×24 nominal). Loaded via CDN from `unpkg.com/heroicons` in HTML files, or rendered inline from the SVGs in `assets/icons/heroicons/` for offline / production use.

- **Stroke weight:** always 1.5px at 24px nominal. Scale uniformly; do not adjust stroke.
- **Color:** always inherit `currentColor`. Never apply gradient or two-tone treatments.
- **Sizing:** common sizes are 16, 20, 24, 32. Icon should equal the surrounding text cap-height + 4px when paired with text.
- **Alignment:** vertically optical-aligned with text, not centered on bounding box.

**Emoji:** never used in product UI. See Content Fundamentals.

**Unicode glyphs:** the following are used as semantic typographic marks (not icons):
- `→` (U+2192) for "see also" and inline navigation hints.
- `·` (U+00B7) as a metadata separator.
- `—` (U+2014) em dash for editorial pauses (always with thin spaces, never spaces).

**Logo:** the **University of Nottingham** master logo is the lead mark on every Matchboard surface (`assets/uon-logo-blue.png` on light, `assets/uon-logo-white.png` on the navy inverse). It pairs with the Matchboard wordmark via a 1px vertical divider — see `preview/brand-logo.html` for the lock-up. Use the official Nottingham brand-portal files in any production build; the screen-resolution PNGs supplied here are placeholders. The DRS mark in `assets/drs-mark.svg` is an internal placeholder and is not the public-facing mark.

---

## Font Substitution Notice

The brand specifies **Geist** (Vercel's open-source typeface, available on Google Fonts and GitHub). Files are loaded via Google Fonts CDN; if you need self-hosted woff2 files, download from the Geist GitHub repo and place in `fonts/`. **No font substitution is currently active** — Geist is the real, intended choice.
